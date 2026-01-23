using Authentication.Api.Contracts;
using Authentication.Api.UseCases.Commands.LoginUser;
using Authentication.Api.UseCases.Commands.RegisterUser;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
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
        public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> Login(LoginRequest loginRequest)
        {
            return await _bus.InvokeAsync<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>>(loginRequest);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest loginRequest)
        {
            await _bus.InvokeAsync<RegisterUserCommand>(new RegisterUserCommand
            {
                Email = loginRequest.Email,
                Password = loginRequest.Password
            });

            return Ok();
        }
    }
}
