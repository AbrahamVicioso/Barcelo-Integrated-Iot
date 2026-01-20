using Authentication.Api.Contracts;
using Authentication.Api.UseCases.Commands.LoginUser;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Authentication.Api.Controllers
{
    [Route("[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMessageBus _bus;

        public AuthController(IMessageBus bus)
        {
            this._bus = bus;
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {   
            await _bus.InvokeAsync<LoginUserCommand>(new LoginUserCommand
            {
                Email = loginRequest.Email,
                Password = loginRequest.Password
            });

            return Ok();
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
