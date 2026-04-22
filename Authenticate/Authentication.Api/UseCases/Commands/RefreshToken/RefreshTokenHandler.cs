using Authentication.Api.Contracts;
using Authentication.Api.DTOs;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Api.UseCases.Commands.RefreshToken;

public class RefreshTokenHandler
{
    public static async Task<Results<Ok<AccessTokenResponse>, ProblemHttpResult>> Handle(
        RefreshTokenRequest request,
        UserManager<User> userManager,
        IJwtGenerator jwtGenerator)
    {
        var userId = jwtGenerator.ValidateRefreshToken(request.RefreshToken);
        if (userId == null)
            return TypedResults.Problem("Refresh token inválido o expirado.", statusCode: StatusCodes.Status401Unauthorized);

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return TypedResults.Problem("Usuario no encontrado.", statusCode: StatusCodes.Status401Unauthorized);

        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, newRefreshToken) = await jwtGenerator.GenerateTokensAsync(roles, user);

        return TypedResults.Ok(new AccessTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 1800
        });
    }
}
