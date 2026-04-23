using Authentication.Api.Services;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Notification.Domain.Events;

namespace Authentication.Api.UseCases.Commands.SendTwoFactorCode
{
    public class SendTwoFactorCodeHandler
    {
        public static async Task<Results<Ok<string>, ProblemHttpResult>> Handle(
            string email,
            UserManager<User> userManager,
            IKafkaProducerService kafkaProducer,
            IConfiguration configuration)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return TypedResults.Ok(string.Empty);
            }

            if (!user.TwoFactorEnabled)
            {
                return TypedResults.Problem("2FA no está habilitado para este usuario.", statusCode: StatusCodes.Status400BadRequest);
            }

            var expirationMinutes = configuration.GetValue<int>("TwoFactorAuth:TokenExpirationMinutes", 5);

            var token = await userManager.GenerateTwoFactorTokenAsync(user, "Email");

            await kafkaProducer.PublishTwoFactorCodeAsync(new TwoFactorCodeEvent
            {
                UserId = user.Id,
                Email = user.Email ?? email,
                Code = token,
                Provider = "Email",
                ExpirationMinutes = expirationMinutes,
                CreatedAt = DateTime.UtcNow
            });

            return TypedResults.Ok("Código enviado exitosamente.");
        }
    }
}