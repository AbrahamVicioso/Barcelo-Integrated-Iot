using Authentication.Api.DTOs;
using Authentication.Api.Services;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Authentication.Api.UseCases.Commands.TwoFactorConfirm
{
    public class TwoFactorConfirmHandler
    {
        public static async Task<Results<Ok<TwoFactorStatusResponse>, ProblemHttpResult>> Handle(
            User user,
            UserManager<User> userManager,
            ITwoFactorCacheService cacheService,
            ILogger<TwoFactorConfirmHandler> logger)
        {
            if (user.TwoFactorEnabled)
            {
                return TypedResults.Ok(new TwoFactorStatusResponse
                {
                    IsTwoFactorEnabled = true,
                    TwoFactorProvider = "Email"
                });
            }

            var cachedCode = cacheService.GetPendingVerification(user.Id);
            if (cachedCode == null)
            {
                logger.LogWarning("No hay código pendiente para usuario {UserId}", user.Id);
                return TypedResults.Problem("No hay código de verificación pendiente. Solicita uno nuevo.", statusCode: StatusCodes.Status400BadRequest);
            }

            cacheService.RemovePendingVerification(user.Id);

            var result = await userManager.SetTwoFactorEnabledAsync(user, true);
            if (!result.Succeeded)
            {
                logger.LogError("Error al activar 2FA para usuario {UserId}: {Errors}", user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
                return TypedResults.Problem("Error al activar 2FA.", statusCode: StatusCodes.Status500InternalServerError);
            }

            logger.LogInformation("2FA activado exitosamente para usuario {UserId}", user.Id);

            return TypedResults.Ok(new TwoFactorStatusResponse
            {
                IsTwoFactorEnabled = true,
                TwoFactorProvider = "Email"
            });
        }
    }
}