using Authentication.Api.Services;
using Authentication.Api.DTOs;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;

namespace Authentication.Api.UseCases.Commands.TwoFactorEnable
{
    public class TwoFactorEnableHandler
    {
        public static async Task<Results<Ok<TwoFactorStatusResponse>, ProblemHttpResult>> Handle(
            User user,
            UserManager<User> userManager,
            IKafkaProducerService kafkaProducer,
            IConfiguration configuration,
            ILogger<TwoFactorEnableHandler> logger)
        {
            if (!await userManager.IsEmailConfirmedAsync(user))
            {
                return TypedResults.Problem("Debes confirmar tu correo electrónico antes de activar 2FA.", statusCode: StatusCodes.Status400BadRequest);
            }

            if (user.TwoFactorEnabled)
            {
                return TypedResults.Ok(new TwoFactorStatusResponse
                {
                    IsTwoFactorEnabled = true,
                    TwoFactorProvider = "Email"
                });
            }

            var token = await userManager.GenerateTwoFactorTokenAsync(user, "Email");
            var expirationMinutes = configuration.GetValue<int>("TwoFactorAuth:TokenExpirationMinutes", 5);

            logger.LogInformation("Enviando código de verificación para activar 2FA usuario {UserId}", user.Id);

            await kafkaProducer.PublishTwoFactorCodeAsync(new TwoFactorCodeEvent
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Code = token,
                Provider = "Email",
                ExpirationMinutes = expirationMinutes,
                CreatedAt = DateTime.UtcNow
            });

            return TypedResults.Ok(new TwoFactorStatusResponse
            {
                IsTwoFactorEnabled = false,
                TwoFactorProvider = "Email"
            });
        }
    }
}