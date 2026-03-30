using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Notification.Kafka.Services;

/// <summary>
/// Internal HTTP client to call Authenticate.API for storing the ntfy access token
/// associated with a user. The token is then exposed only via a JWT-protected endpoint.
/// </summary>
public class AuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthApiClient> _logger;

    public AuthApiClient(HttpClient httpClient, ILogger<AuthApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task StoreNtfyTokenAsync(string email, string ntfyToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(
                "/internal/ntfy-token",
                new { email, ntfyToken },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to store ntfy token for {Email}. Status: {Status}. Body: {Body}",
                    email, response.StatusCode, body);
            }
            else
            {
                _logger.LogInformation("ntfy token stored for {Email}", email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing ntfy token for {Email}", email);
        }
    }
}
