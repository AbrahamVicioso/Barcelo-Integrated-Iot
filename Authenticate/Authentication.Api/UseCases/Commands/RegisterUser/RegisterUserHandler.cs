using Authentication.Api.Services;
using Authentication.Api.Utils.Commons;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.WebUtilities;
using Notification.Domain.Events;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Authentication.Api.UseCases.Commands.RegisterUser
{
    public class RegisterUserHandler
    {
        public static async Task<Results<Ok, ValidationProblem>> Handle(
            RegisterRequest registerRequest,
            UserManager<User> userManager,
            IUserStore<User> userStore,
            IKafkaProducerService kafkaProducerService,
            string confirmEmailBaseUrl,
            string confirmEmailSuffix = "/ConfirmEmail"
            )
        {
            EmailAddressAttribute _emailAddressAttribute = new EmailAddressAttribute();

            if (string.IsNullOrEmpty(registerRequest.Email) || !_emailAddressAttribute.IsValid(registerRequest.Email))
            {
                return IdentityResult.Failed(userManager.ErrorDescriber.InvalidEmail(registerRequest.Email)).ToValidationProblem();
            }

            var user = new User();

            await userManager.SetUserNameAsync(user, registerRequest.Email);
            await userManager.SetEmailAsync(user, registerRequest.Email);

            var result = await userManager.CreateAsync(user, registerRequest.Password);

            if (!result.Succeeded)
            {
                return result.ToValidationProblem();
            }

            // Publish UserCreatedEvent for ntfy account + welcome email
            await kafkaProducerService.PublishUserCreatedAsync(new UserCreatedEvent
            {
                Id = Guid.Parse(user.Id),
                Email = registerRequest.Email,
                GeneratedPassword = string.Empty,
                UserName = registerRequest.Email.Split('@')[0],
                CreatedAt = DateTime.Now
            });

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmationUrl = $"{confirmEmailBaseUrl}{confirmEmailSuffix}?userId={user.Id}&token={encodedToken}";

            await kafkaProducerService.PublishEmailConfirmationAsync(new EmailConfirmationEvent
            {
                UserId = user.Id,
                Email = user.Email!,
                ConfirmationUrl = confirmationUrl
            });

            return TypedResults.Ok();
        }
    }
}
