using Authentication.Api.Services;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace Authentication.Api.UseCases.Commands.RegisterUser
{
    public class RegisterUserHandler
    {

        public static async Task<Results<Ok, ValidationProblem>> Handle(
            RegisterRequest registerRequest,
            UserManager<User> userManager,
            IUserStore<User> userStore
            )
        {
            EmailAddressAttribute _emailAddressAttribute = new EmailAddressAttribute();

            if (string.IsNullOrEmpty(registerRequest.Email) || !_emailAddressAttribute.IsValid(registerRequest.Email))
            {
                return CreateValidationProblem(IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(registerRequest.Email)));
            }

            var user = new User();

            await userManager.SetUserNameAsync(user,registerRequest.Email);
            await userManager.SetEmailAsync(user, registerRequest.Email);

            var result = await userManager.CreateAsync(user, registerRequest.Password);

            if (!result.Succeeded)
            {
                return CreateValidationProblem(result);
            }
            
            //AQUI VA EL SERVICIO DE CONFIMATION EMAIL
            return TypedResults.Ok();
        }
        private static ValidationProblem CreateValidationProblem(string errorCode, string errorDescription) 
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    { errorCode, [errorDescription] }
            });
        }

        private static ValidationProblem CreateValidationProblem(IdentityResult result)
        {
            var errorDictionary = new Dictionary<string, string[]>(1);

            foreach (var error in result.Errors)
            {
                string[] newDescriptions;

                if (errorDictionary.TryGetValue(error.Code, out var desriptions)) {
                    newDescriptions = new string[desriptions.Length + 1];
                    Array.Copy(desriptions, newDescriptions, desriptions.Length);
                    newDescriptions[desriptions.Length] = error.Description;
                }
                else
                {
                    newDescriptions = [error.Description];
                }

                errorDictionary[error.Code] = newDescriptions;
            }

            return TypedResults.ValidationProblem(errorDictionary);
        }
    }
}
