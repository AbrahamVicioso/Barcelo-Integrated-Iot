using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Api.UseCases.Commands.RegisterUser
{
    public class RegisterUserHandler
    {
        public static async Task Handle(RegisterUserCommand registerUserCommand, UserManager<User> userManager)
        {
            var result = await userManager.CreateAsync(new User
            {
                UserName = registerUserCommand.Email,
                Email = registerUserCommand.Email,
            }, registerUserCommand.Password);

            if (!result.Succeeded)
            {
                throw new Exception("Error registering user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
