using Grpc.Contracts.Authentication;
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
            _logger.LogInformation("gRPC: Buscando userId para email: {Email}", email);

            try
            {
                var response = await _client.GetUserIdByEmailAsync(new GetUserIdByEmailRequest { Email = email });

                if (response.Found && !string.IsNullOrEmpty(response.UserId))
                {
                    _logger.LogInformation("gRPC: UserId encontrado para email: {Email}", email);
                    return Guid.Parse(response.UserId);
                }

                _logger.LogWarning("gRPC: Usuario no encontrado para email: {Email}", email);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "gRPC: Error al buscar userId para email: {Email}", email);
                throw;
            }
        }

        public async Task<string?> GetEmailByUserIdAsync(string userId)
        {
            _logger.LogInformation("gRPC: Buscando email para userId: {UserId}", userId);

            try
            {
                var response = await _client.GetEmailByUserIdAsync(new GetEmailByUserIdRequest { UserId = userId });

                if (response.Found)
                {
                    _logger.LogInformation("gRPC: Email encontrado para userId: {UserId}", userId);
                    return response.Email;
                }

                _logger.LogWarning("gRPC: Usuario no encontrado para ID: {UserId}", userId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "gRPC: Error al buscar email para userId: {UserId}", userId);
                return null;
            }
        }

        public Task<Guid> CreateUserAsync(string email, string password)
        {
            throw new NotImplementedException("Use GetUserIdByEmailAsync for guest creation");
        }
    }
}
