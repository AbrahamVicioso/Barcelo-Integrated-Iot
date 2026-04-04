using Authentication.Api.Contracts;
using Authentication.Api.DTOs;
using Authentication.Api.Services;
using Authentication.Api.UseCases.Commands.CreateUserWithRandomPassword;
using Authentication.Api.UseCases.Commands.LoginUser;
using Authentication.Api.UseCases.Commands.RegisterUser;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Notification.Domain.Events;
using Notification.Domain.Interfaces;
using System.Text;

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

        public AuthController(
            UserManager<User> userManager,
            IUserStore<User> userStore,
            IKafkaProducerService kafkaProducerService,
            IAuditProducer auditProducer,
            SignInManager<User> signInManager,
            IJwtGenerator jwtGenerator,
            IConfiguration configuration)
        {
            this.userManager = userManager;
            this.userStore = userStore;
            this.kafkaProducerService = kafkaProducerService;
            this.auditProducer = auditProducer;
            this.signInManager = signInManager;
            this.jwtGenerator = jwtGenerator;
            this.configuration = configuration;
        }

        [HttpPost]
        public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> Login(LoginRequest loginRequest)
        {
            var result = await LoginUserHandler.Handle(loginRequest, signInManager, userManager, jwtGenerator);

            var isSuccess = result.Result is Ok<AccessTokenResponse>;
            var user = isSuccess ? await userManager.FindByEmailAsync(loginRequest.Email) : null;
            await auditProducer.PublishAsync(new AuditEvent
            {
                Servicio = "Authenticate.API",
                UsuarioId = user?.Id,
                Accion = "LOGIN",
                TipoEntidad = "Usuario",
                DireccionIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                AgenteUsuario = Request.Headers["User-Agent"].ToString(),
                Resultado = isSuccess ? "Exitoso" : "Fallido",
                ValorNuevo = loginRequest.Email
            });

            return result;
        }

        [HttpPost]
        public async Task<Results<Ok, ValidationProblem>> Register(RegisterRequest registerRequest)
        {
            var confirmEmailBaseUrl = configuration["ConfirmEmail:BaseUrl"] ?? "http://localhost:5117";
            var result = await RegisterUserHandler.Handle(registerRequest, userManager, userStore, kafkaProducerService, confirmEmailBaseUrl);

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
        public async Task<Results<Ok<CreateUserWithRandomPasswordResponse>, ValidationProblem>> Create([FromBody] EmailRequest request)
        {
            var confirmEmailBaseUrl = configuration["ConfirmEmail:BaseUrl"] ?? "http://localhost:5117";
            return await CreateUserWithRandomPasswordHandler.Handle(request, userManager, kafkaProducerService, confirmEmailBaseUrl);
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

        private static async Task<InfoResponse> CreateInfoResponseAsync<TUser>(TUser user, UserManager<TUser> userManager)
            where TUser : class
        {
            return new()
            {
                Email = await userManager.GetEmailAsync(user) ?? throw new NotSupportedException("Users must have an email."),
                IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user),
            };
        }
    }
}
