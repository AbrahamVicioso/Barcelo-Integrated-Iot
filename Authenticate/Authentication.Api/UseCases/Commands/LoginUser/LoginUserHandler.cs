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
            var result = await _signInManager.PasswordSignInAsync(
                login.Email, 
                login.Password, 
                isPersistent: false, 
                lockoutOnFailure: true
            );

            if (!result.Succeeded)
            {
                return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
            }

            var user = await _userManager.FindByEmailAsync(login.Email);

            if (user == null) 
            {
                return TypedResults.Problem(result.ToString(), statusCode: StatusCodes.Status401Unauthorized);
            }

            var roles = await _userManager.GetRolesAsync(user);

            var token = await jwtGenerator.GenerateJwtToken(
                roles,
                user
            );

            return TypedResults.Ok(new AccessTokenResponse
            {
                AccessToken = token,
                RefreshToken = token,
                ExpiresIn = 3600
            });
        }
    }
}
