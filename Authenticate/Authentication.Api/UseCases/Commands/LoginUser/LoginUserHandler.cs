using Authentication.Api.Contracts;
using Authentication.Api.Services;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;

namespace Authentication.Api.UseCases.Commands.LoginUser
{
    public class LoginUserHandler
    {
        public static async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> Handle(LoginRequest login, SignInManager<User> _signInManager, UserManager<User> _userManager, IJwtGenerator jwtGenerator)
        {
            var user = await _userManager.FindByEmailAsync(login.Email);
            if (user == null)
            {
                return TypedResults.Problem("Credenciales inválidas.", statusCode: StatusCodes.Status401Unauthorized);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                login.Password,
                lockoutOnFailure: true
            );

            if (!result.Succeeded)
            {
                return TypedResults.Problem("Credenciales inválidas.", statusCode: StatusCodes.Status401Unauthorized);
            }

            var roles = await _userManager.GetRolesAsync(user);

            var (accessToken, refreshToken) = await jwtGenerator.GenerateTokensAsync(roles, user);

            return TypedResults.Ok(new AccessTokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 1800
            });
        }
    }
}
