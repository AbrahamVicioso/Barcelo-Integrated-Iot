using Authentication.Api.Services;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Notification.Domain.Events;
using System.Text;

namespace Authentication.Api.UseCases.Commands.ForgotPassword
{
    public class ForgotPasswordHandler
    {
        public static async Task<Ok> Handle(
            string email,
            UserManager<User> userManager,
            IKafkaProducerService kafkaProducerService,
            string resetPasswordBaseUrl)
        {
            // Always return 200 to avoid user enumeration
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var resetUrl = $"{resetPasswordBaseUrl}/ResetPassword?email={Uri.EscapeDataString(email)}&token={encodedToken}";

                await kafkaProducerService.PublishPasswordResetAsync(new PasswordResetEvent
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    ResetUrl = resetUrl
                });
            }

            return TypedResults.Ok();
        }
    }
}
