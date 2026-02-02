using Grpc.Core;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Api.Services
{
    public class UserLookupService : Authentication.Api.Protos.UserLookupService.UserLookupServiceBase
    {
        private readonly UserManager<Authentication.Domain.Entities.User> _userManager;
        private readonly ILogger<UserLookupService> _logger;

        public UserLookupService(
            UserManager<Authentication.Domain.Entities.User> userManager,
            ILogger<UserLookupService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public override async Task<Authentication.Api.Protos.GetUserIdByEmailResponse> GetUserIdByEmail(
            Authentication.Api.Protos.GetUserIdByEmailRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("Looking up user ID for email: {Email}", request.Email);

            if (string.IsNullOrEmpty(request.Email))
            {
                return new Authentication.Api.Protos.GetUserIdByEmailResponse
                {
                    Found = false,
                    UserId = string.Empty
                };
            }

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email}", request.Email);
                return new Authentication.Api.Protos.GetUserIdByEmailResponse
                {
                    Found = false,
                    UserId = string.Empty
                };
            }

            _logger.LogInformation("Found user ID: {UserId} for email: {Email}", user.Id, request.Email);

            return new Authentication.Api.Protos.GetUserIdByEmailResponse
            {
                Found = true,
                UserId = user.Id
            };
        }
    }
}
