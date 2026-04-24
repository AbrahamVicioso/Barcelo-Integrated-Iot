using Authentication.Api.Contracts;
using Authentication.Api.DTOs;
using Authentication.Api.Services;
using Authentication.Api.UseCases.Commands.CreateUserWithRandomPassword;
using Authentication.Api.UseCases.Commands.ForgotPassword;
using Authentication.Api.UseCases.Commands.LoginUser;
using Authentication.Api.UseCases.Commands.RefreshToken;
using Authentication.Api.UseCases.Commands.RegisterUser;
using Authentication.Api.UseCases.Commands.SendTwoFactorCode;
using Authentication.Api.UseCases.Commands.TwoFactorEnable;
using Authentication.Api.UseCases.Commands.TwoFactorDisable;
using Authentication.Api.UseCases.Commands.TwoFactorLogin;
using Authentication.Api.UseCases.Commands.TwoFactorConfirm;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;
using Notification.Domain.Interfaces;
using System.Text;
using Barcelo.Authorization.Shared;

namespace Authentication.Api.Controllers
{
    [Route("[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly IUserStore<User> userStore;
        private readonly IKafkaProducerService kafkaProducerService;
        private readonly IAuditProducer auditProducer;
        private readonly SignInManager<User> signInManager;
        private readonly IJwtGenerator jwtGenerator;
        private readonly IConfiguration configuration;
        private readonly ILogger<AuthController> logger;
        private readonly ILogger<TwoFactorLoginHandler> twoFactorLoginLogger;
        private readonly ILogger<TwoFactorEnableHandler> twoFactorEnableLogger;
        private readonly ILogger<TwoFactorConfirmHandler> twoFactorConfirmLogger;
        private readonly ITwoFactorCacheService twoFactorCacheService;
        private readonly IdentityRuntimeSettings runtimeSettings;

        public AuthController(
            UserManager<User> userManager,
            IUserStore<User> userStore,
            IKafkaProducerService kafkaProducerService,
            IAuditProducer auditProducer,
            SignInManager<User> signInManager,
            IJwtGenerator jwtGenerator,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            ILoggerFactory loggerFactory,
            ITwoFactorCacheService twoFactorCacheService,
            IdentityRuntimeSettings runtimeSettings)
        {
            this.userManager = userManager;
            this.userStore = userStore;
            this.kafkaProducerService = kafkaProducerService;
            this.auditProducer = auditProducer;
            this.signInManager = signInManager;
            this.jwtGenerator = jwtGenerator;
            this.configuration = configuration;
            this.logger = logger;
            this.twoFactorLoginLogger = loggerFactory.CreateLogger<TwoFactorLoginHandler>();
            this.twoFactorEnableLogger = loggerFactory.CreateLogger<TwoFactorEnableHandler>();
            this.twoFactorConfirmLogger = loggerFactory.CreateLogger<TwoFactorConfirmHandler>();
            this.twoFactorCacheService = twoFactorCacheService;
            this.runtimeSettings = runtimeSettings;
        }

        [HttpPost]
        public async Task<IResult> Login(LoginRequest loginRequest)
        {
            var result = await LoginUserHandler.Handle(loginRequest, signInManager, userManager, jwtGenerator, kafkaProducerService, configuration, runtimeSettings);

            var user = await userManager.FindByEmailAsync(loginRequest.Email);
            if (user != null)
            {
                var requiresTwoFactor = result is TwoFactorRequiredResponse;
                await auditProducer.PublishAsync(new AuditEvent
                {
                    Servicio = "Authenticate.API",
                    UsuarioId = user.Id,
                    Accion = requiresTwoFactor ? "LOGIN_2FA_PENDING" : "LOGIN",
                    TipoEntidad = "Usuario",
                    DireccionIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                    AgenteUsuario = Request.Headers["User-Agent"].ToString(),
                    Resultado = (result is Ok<LoginResponse>) ? "Exitoso" : "Fallido",
                    ValorNuevo = loginRequest.Email
                });
            }

            return result;
        }

        [HttpPost]
        public async Task<Results<Ok<AccessTokenResponse>, ProblemHttpResult>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            return await RefreshTokenHandler.Handle(request, userManager, jwtGenerator);
        }

        [HttpPost]
        public async Task<Results<Ok, ValidationProblem>> Register(RegisterRequest registerRequest)
        {
            var confirmEmailBaseUrl = BuildConfirmEmailBaseUrl();
            var result = await RegisterUserHandler.Handle(registerRequest, userManager, userStore, kafkaProducerService, confirmEmailBaseUrl, string.Empty);

            var isSuccess = result.Result is Ok;
            var user = isSuccess ? await userManager.FindByEmailAsync(registerRequest.Email) : null;
            await auditProducer.PublishAsync(new AuditEvent
            {
                Servicio = "Authenticate.API",
                UsuarioId = user?.Id,
                Accion = "REGISTER",
                TipoEntidad = "Usuario",
                DireccionIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                AgenteUsuario = Request.Headers["User-Agent"].ToString(),
                Resultado = isSuccess ? "Exitoso" : "Fallido",
                ValorNuevo = registerRequest.Email
            });

            return result;
        }

        [HttpPost]
        [HasPermission(Permissions.Auth.Create)]
        public async Task<Results<Ok<CreateUserWithRandomPasswordResponse>, ValidationProblem>> Create([FromBody] EmailRequest request)
        {
            var confirmEmailBaseUrl = BuildConfirmEmailBaseUrl();
            return await CreateUserWithRandomPasswordHandler.Handle(request, userManager, kafkaProducerService, confirmEmailBaseUrl, string.Empty);
        }

        [Authorize]
        [HttpGet]
        public async Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>> Info()
        {
            if (await userManager.GetUserAsync(HttpContext?.User) is not { } user)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
        }

        [HttpGet]
        [HasPermission(Permissions.Auth.View)]
        public async Task<Results<Ok<UserInfoResponse>, NotFound, ProblemHttpResult>> GetUserByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return TypedResults.Problem("Email is required.");
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return TypedResults.NotFound();
            }

            var response = new UserInfoResponse
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user)
            };

            return TypedResults.Ok(response);
        }

        [HttpPost]
        public async Task<Ok> ForgotPassword([FromBody] EmailRequest request)
        {
            var baseUrl = BuildForgotPasswordBaseUrl();
            var resetPasswordSuffix = configuration["ForgotPassword:Suffix"] ?? "/ResetPassword";
            return await ForgotPasswordHandler.Handle(request.Email, userManager, kafkaProducerService, baseUrl, resetPasswordSuffix);
        }

        [HttpPost]
        public async Task<Results<Ok<string>, ValidationProblem, BadRequest<string>>> ResetPassword([FromBody] ResetPasswordDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
                return TypedResults.BadRequest("Email, token y nueva contraseña son requeridos.");

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return TypedResults.BadRequest("Datos inválidos.");

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
                return TypedResults.ValidationProblem(errors);
            }

            return TypedResults.Ok("Contraseña restablecida exitosamente.");
        }

        [HttpGet]
        public async Task<Results<Ok<string>, BadRequest<string>>> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return TypedResults.BadRequest("userId y token son requeridos.");

            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return TypedResults.BadRequest("Usuario no encontrado.");

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
                return TypedResults.BadRequest("El token de confirmación es inválido o ha expirado.");

            return TypedResults.Ok("Correo electrónico confirmado exitosamente.");
        }

        [HttpPost]
        public async Task<IResult> LoginVerifyTwoFactor([FromBody] TwoFactorLoginRequest request)
        {
            return await TwoFactorLoginHandler.Handle(request, userManager, signInManager, jwtGenerator, twoFactorLoginLogger);
        }

        [HttpPost]
        public async Task<Results<Ok<string>, ProblemHttpResult>> LoginSendTwoFactorCode([FromBody] EmailRequest request)
        {
            return await SendTwoFactorCodeHandler.Handle(request.Email, userManager, kafkaProducerService, configuration);
        }

        [Authorize]
        [HttpGet]
        public async Task<Results<Ok<TwoFactorStatusResponse>, NotFound>> TwoFactorStatus()
        {
            if (await userManager.GetUserAsync(HttpContext?.User) is not { } user)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new TwoFactorStatusResponse
            {
                IsTwoFactorEnabled = user.TwoFactorEnabled,
                TwoFactorProvider = user.TwoFactorEnabled ? "Email" : string.Empty
            });
        }

        [Authorize]
        [HttpPost]
        public async Task<Results<Ok<TwoFactorStatusResponse>, ProblemHttpResult>> TwoFactorEnable()
        {
            if (await userManager.GetUserAsync(HttpContext?.User) is not { } user)
            {
                return TypedResults.Problem("Usuario no encontrado.", statusCode: StatusCodes.Status404NotFound);
            }

            return await TwoFactorEnableHandler.Handle(user, userManager, kafkaProducerService, twoFactorCacheService, configuration, twoFactorEnableLogger);
        }

        [Authorize]
        [HttpPost]
        public async Task<Results<Ok<TwoFactorStatusResponse>, ProblemHttpResult>> TwoFactorConfirm([FromBody] TwoFactorConfirmRequest request)
        {
            if (await userManager.GetUserAsync(HttpContext?.User) is not { } user)
            {
                return TypedResults.Problem("Usuario no encontrado.", statusCode: StatusCodes.Status404NotFound);
            }

            return await TwoFactorConfirmHandler.Handle(user, userManager, twoFactorCacheService, twoFactorConfirmLogger);
        }

        [Authorize]
        [HttpPost]
        public async Task<Results<Ok<TwoFactorStatusResponse>, ProblemHttpResult>> TwoFactorDisable([FromBody] TwoFactorDisableRequest request)
        {
            if (await userManager.GetUserAsync(HttpContext?.User) is not { } user)
            {
                return TypedResults.Problem("Usuario no encontrado.", statusCode: StatusCodes.Status404NotFound);
            }

            return await TwoFactorDisableHandler.Handle(user, userManager, request.Password);
        }

        private static async Task<InfoResponse> CreateInfoResponseAsync<TUser>(TUser user, UserManager<TUser> userManager)
            where TUser : class
        {
            return new()
            {
                Email = await userManager.GetEmailAsync(user) ?? throw new NotSupportedException("Users must have an email."),
                IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user),
            };
        }

        private string BuildConfirmEmailBaseUrl()
        {
            var domain = configuration["ConfirmEmail:Domain"] ?? "smartstay.int";
            return $"https://{domain}/confirm-email";
        }

        private string BuildForgotPasswordBaseUrl()
        {
            var forwardedHost = Request.Headers["X-Forwarded-Host"].FirstOrDefault();
            var forwardedProto = Request.Headers["X-Forwarded-Proto"].FirstOrDefault();

            if (!string.IsNullOrEmpty(forwardedHost))
            {
                var proto = forwardedProto ?? Request.Scheme;
                var pathPrefix = configuration["ForgotPassword:PathPrefix"] ?? "/api/auth";
                return $"{proto}://{forwardedHost}{pathPrefix}";
            }

            return configuration["ForgotPassword:BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
        }
    }
}
