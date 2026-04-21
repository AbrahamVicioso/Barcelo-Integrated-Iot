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

    public async Task<string?> GetUserIdByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("GetUserIdByEmailAsync called with empty email");
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync($"/getuserbyemail?email={Uri.EscapeDataString(email)}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get user ID for email {Email}. Status: {Status}", email, response.StatusCode);
                return null;
            }

            var userInfo = await response.Content.ReadFromJsonAsync<UserInfoResponse>(cancellationToken: cancellationToken);

            if (userInfo == null)
            {
                _logger.LogWarning("UserInfo response is null for email {Email}", email);
                return null;
            }

            _logger.LogDebug("User ID {UserId} obtained for email {Email}", userInfo.Id, email);
            return userInfo.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user ID for email {Email}", email);
            return null;
        }
    }
}

public class UserInfoResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsEmailConfirmed { get; set; }
}
