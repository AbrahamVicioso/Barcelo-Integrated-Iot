using Authentication.Api.Protos;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Usuarios.Domain.Interfaces;

namespace Usuarios.ExternalService.Repositories
{
    public class AuthenticationGrpcClient : IAuthenticationApiClient
    {
        private readonly UserLookup.UserLookupClient _client;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthenticationGrpcClient> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public AuthenticationGrpcClient(GrpcChannel channel, HttpClient httpClient, ILogger<AuthenticationGrpcClient> logger)
        {
            _client = new UserLookup.UserLookupClient(channel);
            _httpClient = httpClient;
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

        public async Task<string?> GetEmailByUserIdAsync(string userId)
        {
            _logger.LogInformation("Looking up email for user ID: {UserId}", userId);

            try
            {
                var response = await _httpClient.GetAsync($"users/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("User not found for ID: {UserId}, status: {Status}", userId, response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<UserResponse>(content, _jsonOptions);

                _logger.LogInformation("Found email for user ID: {UserId}", userId);
                return user?.Email;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up email for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<Guid> CreateUserAsync(string email, string password)
        {
            throw new NotImplementedException("Use GetUserIdByEmailAsync for guest creation");
        }

        private class UserResponse
        {
            public string Email { get; set; } = string.Empty;
        }
    }
}
