using Grpc.Contracts.Authentication;
using Grpc.Core;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Api.Services
{
    public class UserLookupService : UserLookup.UserLookupBase
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

        public override async Task<GetUserIdByEmailResponse> GetUserIdByEmail(
            GetUserIdByEmailRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Buscando userId para email: {Email}", request.Email);

            if (string.IsNullOrEmpty(request.Email))
                return new GetUserIdByEmailResponse { Found = false, UserId = string.Empty };

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                _logger.LogWarning("gRPC: Usuario no encontrado para email: {Email}", request.Email);
                return new GetUserIdByEmailResponse { Found = false, UserId = string.Empty };
            }

            _logger.LogInformation("gRPC: UserId encontrado: {UserId} para email: {Email}", user.Id, request.Email);
            return new GetUserIdByEmailResponse { Found = true, UserId = user.Id };
        }

        public override async Task<GetEmailByUserIdResponse> GetEmailByUserId(
            GetEmailByUserIdRequest request,
            ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Buscando email para userId: {UserId}", request.UserId);

            if (string.IsNullOrEmpty(request.UserId))
                return new GetEmailByUserIdResponse { Found = false, Email = string.Empty };

            var user = await _userManager.FindByIdAsync(request.UserId);

            if (user == null)
            {
                _logger.LogWarning("gRPC: Usuario no encontrado para ID: {UserId}", request.UserId);
                return new GetEmailByUserIdResponse { Found = false, Email = string.Empty };
            }

            _logger.LogInformation("gRPC: Email encontrado para userId: {UserId}", request.UserId);
            return new GetEmailByUserIdResponse { Found = true, Email = user.Email ?? string.Empty };
        }
    }
}
