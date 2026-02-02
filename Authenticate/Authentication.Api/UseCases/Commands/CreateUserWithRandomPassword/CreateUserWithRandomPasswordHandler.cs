using Authentication.Api.DTOs;
using Authentication.Api.Services;
using Authentication.Api.Utils;
using Authentication.Api.Utils.Commons;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Authentication.Api.UseCases.Commands.CreateUserWithRandomPassword
{
    public class CreateUserWithRandomPasswordHandler
    {
        public static async Task<Results<Ok<CreateUserWithRandomPasswordResponse>, ValidationProblem>> Handle(
            EmailRequest request,
            UserManager<User> userManager,
            IKafkaProducerService kafkaProducerService
            )
        {
            EmailAddressAttribute _emailAddressAttribute = new EmailAddressAttribute();

            if (string.IsNullOrEmpty(request.Email) || !_emailAddressAttribute.IsValid(request.Email))
            {
                var errorDictionary = new Dictionary<string, string[]>
                {
                    { "Email", new[] { "Invalid email address." } }
                };
                return TypedResults.ValidationProblem(errorDictionary);
            }

            // Check if user already exists
            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                var response = new CreateUserWithRandomPasswordResponse
                {
                    Email = request.Email,
                    IsSuccess = false,
                    ErrorMessage = "User with this email already exists."
                };
                return TypedResults.Ok(response);
            }

            // Generate a random password (not too long, default 10 characters)
            string randomPassword = RandomPasswordGenerator.Generate(10);

            // Create new user
            var user = new User();

            await userManager.SetUserNameAsync(user, request.Email);

            await userManager.SetEmailAsync(user, request.Email);

            var result = await userManager.CreateAsync(user, randomPassword);

            if (!result.Succeeded)
            {
                return result.ToValidationProblem();
            }

            // Publish UserCreatedEvent to Kafka for email notification
            var userCreatedEvent = new Notification.Domain.Events.UserCreatedEvent
            {
                Id = Guid.Parse(user.Id),
                Email = request.Email,
                GeneratedPassword = randomPassword,
                UserName = request.Email.Split('@')[0],
                CreatedAt = DateTime.UtcNow
            };

            await kafkaProducerService.PublishUserCreatedAsync(userCreatedEvent);

            var responseSuccess = new CreateUserWithRandomPasswordResponse
            {
                Email = request.Email,
                Id = user.Id,
                IsSuccess = true
            };

            return TypedResults.Ok(responseSuccess);
        }
    }
}
