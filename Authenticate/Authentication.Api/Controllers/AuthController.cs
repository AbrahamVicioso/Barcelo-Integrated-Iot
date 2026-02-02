using Authentication.Api.Contracts;
using Authentication.Api.DTOs;
using Authentication.Api.UseCases.Commands.CreateOrGetUser;
using Authentication.Api.UseCases.Commands.CreateUserWithRandomPassword;
using Authentication.Api.UseCases.Commands.LoginUser;
using Authentication.Api.UseCases.Commands.RegisterUser;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Wolverine;

namespace Authentication.Api.Controllers
{
    [Route("[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMessageBus _bus;
        private readonly UserManager<User> userManager;

        public AuthController(IMessageBus bus, UserManager<User> userManager)
        {
            this._bus = bus;
            this.userManager = userManager;
        }

        [HttpPost]
        public async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> Login(LoginRequest loginRequest)
        {
            return await _bus.InvokeAsync<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>>(loginRequest);
        }

        [HttpPost]
        public async Task<Results<Ok, ValidationProblem>> Register(RegisterRequest registerRequest)
        {
            return await _bus.InvokeAsync<Results<Ok, ValidationProblem>> (registerRequest);
        }

        [HttpPost]
        public async Task<Results<Ok<CreateUserWithRandomPasswordResponse>, ValidationProblem>> Create([FromBody] EmailRequest request)
        {
            return await CreateUserWithRandomPasswordHandler.Handle(request, userManager);
        }

        [Authorize]
        [HttpGet]
        public async Task<Results<Ok<InfoResponse>, ValidationProblem, NotFound>> Info()
        {
            if (await userManager.GetUserAsync(HttpContext?.User) is not { } user)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(await CreateInfoResponseAsync(user, userManager));
        }

        [HttpGet]
        public async Task<Results<Ok<UserInfoResponse>, NotFound, ProblemHttpResult>> GetUserByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return TypedResults.Problem("Email is required.");
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return TypedResults.NotFound();
            }

            var response = new UserInfoResponse
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user)
            };

            return TypedResults.Ok(response);
        }

        private static async Task<InfoResponse> CreateInfoResponseAsync<TUser>(TUser user, UserManager<TUser> userManager)
            where TUser : class
        {
            return new()
            {
                Email = await userManager.GetEmailAsync(user) ?? throw new NotSupportedException("Users must have an email."),
                IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user),
            };
        }
    }
}
