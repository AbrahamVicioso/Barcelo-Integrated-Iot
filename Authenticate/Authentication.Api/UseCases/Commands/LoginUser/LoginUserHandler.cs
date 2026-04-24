using Authentication.Api.Contracts;
using Authentication.Api.Services;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Configuration;
using Notification.Domain.Events;
using System.Text.Json;

namespace Authentication.Api.UseCases.Commands.LoginUser
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }

    public class TwoFactorRequiredResponse : IResult
    {
        public bool RequiresTwoFactor { get; set; } = true;
        public string TwoFactorProvider { get; set; } = "Email";
        public string Message { get; set; } = string.Empty;

        public static Type ResultType => typeof(TwoFactorRequiredResponse);

        public async Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = StatusCodes.Status200OK;
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }

    public class LoginUserHandler
    {
        public static async Task<IResult> Handle(
            LoginRequest login,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IJwtGenerator jwtGenerator,
            IKafkaProducerService kafkaProducer,
            IConfiguration configuration,
            Authentication.Api.Services.IdentityRuntimeSettings? runtimeSettings = null)
        {
            var user = await userManager.FindByEmailAsync(login.Email);
            if (user == null)
            {
                return TypedResults.Problem("Credenciales inválidas.", statusCode: StatusCodes.Status401Unauthorized);
            }

            if (!await userManager.IsEmailConfirmedAsync(user))
            {
                return TypedResults.Problem("Debes confirmar tu correo electrónico antes de iniciar sesión.", statusCode: StatusCodes.Status401Unauthorized);
            }

            var result = await signInManager.CheckPasswordSignInAsync(
                user,
                login.Password,
                lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                return TypedResults.Problem("Credenciales inválidas.", statusCode: StatusCodes.Status401Unauthorized);
            }

            var roles = await userManager.GetRolesAsync(user);

            // 2FA: siempre si está habilitado; o si la configuración lo requiere para admins
            var isAdmin = roles.Contains("Admin");
            var requiresTwoFactor = user.TwoFactorEnabled ||
                (isAdmin && (runtimeSettings?.TwoFactorRequireForAdmins ?? false));

            if (requiresTwoFactor)
            {
                var token = await userManager.GenerateTwoFactorTokenAsync(user, "Email");
                var expirationMinutes = configuration.GetValue<int>("TwoFactorAuth:TokenExpirationMinutes", 5);

                await kafkaProducer.PublishTwoFactorCodeAsync(new TwoFactorCodeEvent
                {
                    UserId = user.Id,
                    Email = user.Email ?? login.Email,
                    Code = token,
                    Provider = "Email",
                    ExpirationMinutes = expirationMinutes,
                    CreatedAt = DateTime.UtcNow
                });

                return new TwoFactorRequiredResponse
                {
                    RequiresTwoFactor = true,
                    TwoFactorProvider = "Email",
                    Message = "Se ha enviado un código de verificación a tu correo electrónico."
                };
            }

            var (accessToken, refreshToken) = await jwtGenerator.GenerateTokensAsync(roles, user);
            var expiresIn = (runtimeSettings?.TokenExpirationMinutes ?? 30) * 60;

            return TypedResults.Ok(new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = expiresIn
            });
        }
    }
}