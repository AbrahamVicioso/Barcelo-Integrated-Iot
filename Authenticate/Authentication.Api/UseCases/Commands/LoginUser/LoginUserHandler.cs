using Authentication.Api.Contracts;
using Authentication.Api.Services;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Api.UseCases.Commands.LoginUser
{
    public class LoginUserHandler
    {
        public static async Task Handle(LoginUserCommand command, UserManager<User> _userManager, IJwtGenerator jwtGenerator)
        {
            var user = await _userManager.FindByEmailAsync(command.Email);

            if (user == null)
            {
                throw new UnauthorizedAccessException();
            }

            var result = await _userManager.CheckPasswordAsync(user, command.Password);

            if (!result) throw new UnauthorizedAccessException();

            var roles = await _userManager.GetRolesAsync(user);

            var token = await jwtGenerator.GenerateJwtToken(roles, user);
        }
    }
}
