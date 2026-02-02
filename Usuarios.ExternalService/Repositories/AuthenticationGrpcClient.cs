using Authentication.Api.Protos;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Usuarios.Domain.Interfaces;

namespace Usuarios.ExternalService.Repositories
{
    public class AuthenticationGrpcClient : IAuthenticationApiClient
    {
        private readonly UserLookup.UserLookupClient _client;
        private readonly ILogger<AuthenticationGrpcClient> _logger;

        public AuthenticationGrpcClient(GrpcChannel channel, ILogger<AuthenticationGrpcClient> logger)
        {
            _client = new UserLookup.UserLookupClient(channel);
            _logger = logger;
        }

        public async Task<Guid?> GetUserIdByEmailAsync(string email)
        {
            _logger.LogInformation("Looking up user ID for email: {Email}", email);

            try
            {
                var request = new GetUserIdByEmailRequest
                {
                    Email = email
                };

                var response = await _client.GetUserIdByEmailAsync(request);

                if (response.Found && !string.IsNullOrEmpty(response.UserId))
                {
                    _logger.LogInformation("Found user ID: {UserId} for email: {Email}", response.UserId, email);
                    return Guid.Parse(response.UserId);
                }

                _logger.LogWarning("User not found for email: {Email}", email);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up user ID for email: {Email}", email);
                throw;
            }
        }

        public async Task<Guid> CreateUserAsync(string email, string password)
        {
            // This method can be implemented if needed, but for now we'll focus on GetUserIdByEmail
            throw new NotImplementedException("Use GetUserIdByEmailAsync for guest creation");
        }
    }
}
