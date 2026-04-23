using Authentication.Api.Contracts;
using Authentication.Api.DTOs;
using Authentication.Api.Services;
using Authentication.Api.UseCases.Commands.LoginUser;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Authentication.Api.UseCases.Commands.TwoFactorLogin
{
    public class TwoFactorLoginHandler
    {
        public static async Task<IResult> Handle(
            TwoFactorLoginRequest request,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtGenerator jwtGenerator,
            ILogger<TwoFactorLoginHandler> logger)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return TypedResults.Problem("Credenciales inválidas.", statusCode: StatusCodes.Status401Unauthorized);
            }

            if (!user.TwoFactorEnabled)
            {
                return TypedResults.Problem("2FA no está habilitado para este usuario.", statusCode: StatusCodes.Status400BadRequest);
            }

            var isValidToken = await userManager.VerifyTwoFactorTokenAsync(user, "Email", request.Code);
            if (!isValidToken)
            {
                logger.LogWarning("Intento de 2FA fallido para usuario {Email}", request.Email);
                return TypedResults.Problem("Código de verificación inválido o expirado.", statusCode: StatusCodes.Status401Unauthorized);
            }

            var roles = await userManager.GetRolesAsync(user);
            var (accessToken, refreshToken) = await jwtGenerator.GenerateTokensAsync(roles, user);

            logger.LogInformation("Login 2FA exitoso para usuario {Email}", request.Email);

            return TypedResults.Ok(new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 1800
            });
        }
    }
}