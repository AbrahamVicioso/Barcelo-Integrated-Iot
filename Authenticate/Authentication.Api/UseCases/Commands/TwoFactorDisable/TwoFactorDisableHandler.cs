using Authentication.Api.Services;
using Authentication.Api.DTOs;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Api.UseCases.Commands.TwoFactorDisable
{
    public class TwoFactorDisableHandler
    {
        public static async Task<Results<Ok<TwoFactorStatusResponse>, ProblemHttpResult>> Handle(
            User user,
            UserManager<User> userManager,
            string password)
        {
            if (!user.TwoFactorEnabled)
            {
                return TypedResults.Ok(new TwoFactorStatusResponse
                {
                    IsTwoFactorEnabled = false,
                    TwoFactorProvider = string.Empty
                });
            }

            var isPasswordValid = await userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
            {
                return TypedResults.Problem("Contraseña incorrecta.", statusCode: StatusCodes.Status400BadRequest);
            }

            user.TwoFactorEnabled = false;
            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return TypedResults.Problem("Error al desactivar 2FA.", statusCode: StatusCodes.Status500InternalServerError);
            }

            return TypedResults.Ok(new TwoFactorStatusResponse
            {
                IsTwoFactorEnabled = false,
                TwoFactorProvider = string.Empty
            });
        }
    }
}