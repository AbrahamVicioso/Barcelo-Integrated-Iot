using Authentication.Api.Services;
using Authentication.Api.Utils.Commons;
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
                return IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(registerRequest.Email)).ToValidationProblem();
            }

            var user = new User();

            await userManager.SetUserNameAsync(user,registerRequest.Email);
            await userManager.SetEmailAsync(user, registerRequest.Email);

            var result = await userManager.CreateAsync(user, registerRequest.Password);

            if (!result.Succeeded)
            {
                return result.ToValidationProblem();
            }
            
            //AQUI VA EL SERVICIO DE CONFIMATION EMAIL
            return TypedResults.Ok();
        }
    }
}
