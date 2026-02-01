using Authentication.Api.Utils.Commons;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Authentication.Api.UseCases.Commands.CreateOrGetUser
{
    public class CreateOrGetUserHandler
    {
        public static async Task<Results<Ok<string>, ValidationProblem>> Handle(
            CreateOrGetUserRequest request,
            UserManager<User> userManager
            )
        {
            EmailAddressAttribute _emailAddressAttribute = new EmailAddressAttribute();

            if (string.IsNullOrEmpty(request.Email) || !_emailAddressAttribute.IsValid(request.Email))
            {
                return IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(request.Email)).ToValidationProblem();
            }

            // Check if user already exists
            var existingUser = await userManager.FindByEmailAsync(request.Email);

            if (existingUser != null)
            {
                // User exists, return the user ID
                return TypedResults.Ok(existingUser.Id);
            }

            // Create new user
            var user = new User();

            await userManager.SetUserNameAsync(user, request.Email);
            await userManager.SetEmailAsync(user, request.Email);

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return result.ToValidationProblem();
            }

            return TypedResults.Ok(user.Id);
        }
    }
}
