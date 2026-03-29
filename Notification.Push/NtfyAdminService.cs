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

            // 1. Create the ntfy user
            var userCreated = await CreateNtfyUserAsync(username, password, cancellationToken);
            if (!userCreated)
            {
                _logger.LogWarning("Could not create ntfy user for {Email}. Push subscription may not work.", email);
                return null;
            }

            // 2. Grant read-only access to their personal topic
            var accessGranted = await GrantReadAccessAsync(username, topic, cancellationToken);
            if (!accessGranted)
                _logger.LogWarning("Could not grant read access for ntfy user {Username} on topic {Topic}.", username, topic);

            _logger.LogInformation("ntfy account created for {Email} with username {Username}", email, username);
            return password;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ntfy account for {Email}", email);
            return null;
        }
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

        // 409 = user already exists, treat as success
        if ((int)response.StatusCode == 409)
        {
            _logger.LogInformation("ntfy user {Username} already exists, skipping creation.", username);
            return true;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError("Failed to create ntfy user {Username}. Status: {Status}. Body: {Body}",
            username, response.StatusCode, body);
        return false;
    }

    private async Task<bool> GrantReadAccessAsync(string username, string topic, CancellationToken cancellationToken)
    {
        var url = $"{_options.BaseUrl.TrimEnd('/')}/v1/access";

        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = JsonContent.Create(new { username, topic, permission = "read-only" })
        };

        AddAdminAuth(request);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
            return true;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError("Failed to grant read access for {Username} on topic {Topic}. Status: {Status}. Body: {Body}",
            username, topic, response.StatusCode, body);
        return false;
    }

    private void AddAdminAuth(HttpRequestMessage request)
    {
        if (!string.IsNullOrEmpty(_options.AccessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);
        else if (!string.IsNullOrEmpty(_options.AdminPassword))
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"admin:{_options.AdminPassword}")));
    }

    private static string GenerateSecurePassword()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%";
        return RandomNumberGenerator.GetString(chars, 24);
    }
}
