using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Notification.Domain.Helpers;
using Notification.Domain.Interfaces;

namespace Notification.Push;

/// <summary>
/// Manages ntfy user accounts via the admin API.
/// Each user gets their own account with read-only access to their personal topic.
/// Returns a long-lived ntfy access token (not a password) so clients never need
/// to know the underlying password — only the token is exposed via the API.
/// The server token (admin) is the only one that can publish.
/// </summary>
public class NtfyAdminService : Notification.Domain.Interfaces.INtfyAdminService
{
    private readonly HttpClient _httpClient;
    private readonly NtfyOptions _options;
    private readonly ILogger<NtfyAdminService> _logger;

    public NtfyAdminService(
        HttpClient httpClient,
        NtfyOptions options,
        ILogger<NtfyAdminService> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<string?> CreateUserAccountAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var username = NtfyTopicHelper.GetUserTopic(email); // e.g. barcelo-user-at-hotel-com
            var password = GenerateSecurePassword();
            var topic = username; // topic name == username (1:1)

            // 1. Create the ntfy user (or reset password if it already exists)
            var userCreated = await CreateOrResetNtfyUserAsync(username, password, cancellationToken);
            if (!userCreated)
            {
                _logger.LogWarning("Could not create ntfy user for {Email}. Push subscription may not work.", email);
                return null;
            }

            // 2. Grant read-only access to their personal topic
            var (accessGranted, grantError) = await GrantReadAccessAsync(username, topic, cancellationToken);
            if (!accessGranted)
            {
                _logger.LogError(
                    "Could not grant read access for ntfy user {Username} on topic {Topic}. Reason: {Reason}",
                    username, topic, grantError);
                return null;
            }

            // 3. Create a long-lived access token authenticated as the new user.
            //    The password is only used here to obtain the token and is then discarded.
            var accessToken = await CreateUserTokenAsync(username, password, cancellationToken);
            if (accessToken is null)
            {
                _logger.LogWarning("Could not create ntfy access token for {Username}.", username);
                return null;
            }

            _logger.LogInformation("ntfy account + access token created for {Email}", email);
            return accessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ntfy account for {Email}", email);
            return null;
        }
    }

    private async Task<string?> CreateUserTokenAsync(string username, string password, CancellationToken cancellationToken)
    {
        var url = $"{_options.BaseUrl.TrimEnd('/')}/v1/account/token";

        // expires is omitted intentionally — ntfy treats a missing/null expires as "never expires".
        // expires = 0 would mean Unix epoch (Jan 1970) and the token would be immediately expired.
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(new { label = "barcelo-app" })
        };

        // Authenticate as the user (not admin) to create a token scoped to that user
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to create ntfy token for {Username}. Status: {Status}. Body: {Body}",
                username, response.StatusCode, body);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<NtfyTokenResponse>(cancellationToken: cancellationToken);
        return result?.Token;
    }

    /// <summary>
    /// Creates the ntfy user. If the user already exists (409), resets their password so
    /// the caller can use the new password to create an access token.
    /// </summary>
    private async Task<bool> CreateOrResetNtfyUserAsync(string username, string password, CancellationToken cancellationToken)
    {
        var url = $"{_options.BaseUrl.TrimEnd('/')}/v1/users";

        var createRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(new { username, password, role = "user" })
        };
        AddAdminAuth(createRequest);

        var response = await _httpClient.SendAsync(createRequest, cancellationToken);

        if (response.IsSuccessStatusCode)
            return true;

        // 409 = user already exists. Delete and recreate so we can authenticate
        // with the new password and generate a fresh token (handles at-least-once Kafka delivery).
        if ((int)response.StatusCode == 409)
        {
            _logger.LogInformation("ntfy user {Username} already exists. Deleting and recreating.", username);
            var deleted = await DeleteNtfyUserAsync(username, cancellationToken);
            if (!deleted) return false;
            return await CreateNtfyUserAsync(username, password, cancellationToken);
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError("Failed to create ntfy user {Username}. Status: {Status}. Body: {Body}",
            username, response.StatusCode, body);
        return false;
    }

    private async Task<bool> DeleteNtfyUserAsync(string username, CancellationToken cancellationToken)
    {
        var url = $"{_options.BaseUrl.TrimEnd('/')}/v1/users";

        var request = new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Content = JsonContent.Create(new { username })
        };
        AddAdminAuth(request);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
            return true;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError("Failed to delete ntfy user {Username}. Status: {Status}. Body: {Body}",
            username, response.StatusCode, body);
        return false;
    }

    private async Task<bool> CreateNtfyUserAsync(string username, string password, CancellationToken cancellationToken)
    {
        var url = $"{_options.BaseUrl.TrimEnd('/')}/v1/users";

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(new { username, password, role = "user" })
        };
        AddAdminAuth(request);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
            return true;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError("Failed to create ntfy user {Username}. Status: {Status}. Body: {Body}",
            username, response.StatusCode, body);
        return false;
    }

    private async Task<(bool Success, string? ErrorDetail)> GrantReadAccessAsync(string username, string topic, CancellationToken cancellationToken)
    {
        var url = $"{_options.BaseUrl.TrimEnd('/')}/v1/users/access";

        var hasAuth = !string.IsNullOrEmpty(_options.AccessToken) || !string.IsNullOrEmpty(_options.AdminPassword);
        if (!hasAuth)
            return (false, "no admin credentials configured (NTFY_SERVER_TOKEN and NTFY_ADMIN_PASSWORD are both empty)");

        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(new { username, topic, permission = "read-only" })
        };

        AddAdminAuth(request);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
            return (true, null);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return (false, $"HTTP {(int)response.StatusCode} {response.StatusCode} — {body}");
    }

    private void AddAdminAuth(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(_options.AdminPassword))
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"admin:{_options.AdminPassword}")));
        else if (!string.IsNullOrEmpty(_options.AccessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);
    }

    public async Task GrantSystemTopicAccessAsync(string ntfyUsername, CancellationToken cancellationToken = default)
    {
        var (granted, error) = await GrantReadAccessAsync(ntfyUsername, _options.SystemTopic, cancellationToken);
        if (!granted)
            _logger.LogWarning("Could not grant system topic access for ntfy user {Username}. Reason: {Reason}", ntfyUsername, error);
        else
            _logger.LogInformation("System topic access granted to {Username}", ntfyUsername);
    }

    private static string GenerateSecurePassword()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%";
        return RandomNumberGenerator.GetString(chars, 24);
    }

    private sealed record NtfyTokenResponse(string Token);
}
