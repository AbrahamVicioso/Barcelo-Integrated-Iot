using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Dispositivos.Application.Interfaces;
using Dispositivos.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Dispositivos.Infrastructure.Services;

/// <summary>
/// Implementation of Thingsboard device management service using HTTP API
/// </summary>
public class TbDeviceService : ITbDeviceService
{
    private readonly HttpClient _httpClient;
    private readonly ThingsboardOptions _options;
    private string? _cachedToken;
    private DateTime _tokenExpiration = DateTime.MinValue;

    public TbDeviceService(
        HttpClient httpClient,
        IOptions<ThingsboardOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<TbDeviceResponse> CreateOrUpdateDeviceAsync(
        string? deviceId,
        string deviceName,
        string deviceType,
        string? label = null,
        string? accessToken = null,
        CancellationToken cancellationToken = default)
    {
        var token = await GetValidTokenAsync(cancellationToken);

        var queryParams = new List<string>();

        if (!string.IsNullOrEmpty(accessToken))
        {
            queryParams.Add($"accessToken={accessToken}");
        }

        var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
        var url = $"/api/device{queryString}";

        // Build device object
        // For NEW devices (no deviceId), don't include ID - Thingsboard will generate one
        // For UPDATES (deviceId provided), include ID in the request body
        object device;
        if (!string.IsNullOrEmpty(deviceId))
        {
            // Update existing device - include ID in body
            device = new
            {
                id = new { entityType = "DEVICE", id = deviceId },
                name = deviceName,
                type = deviceType,
                label = label
            };
        }
        else
        {
            // Create new device - don't include ID, let Thingsboard generate it
            device = new
            {
                name = deviceName,
                type = deviceType,
                label = label
            };
        }

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(
                JsonConvert.SerializeObject(device),
                Encoding.UTF8,
                "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Failed to create/update device in Thingsboard. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var thingsboardResponse = JsonConvert.DeserializeObject<ThingsboardDeviceResponse>(responseContent)
                                ?? throw new InvalidOperationException("Failed to deserialize Thingsboard response");

        return new TbDeviceResponse
        {
            Id = thingsboardResponse.Id?.Id ?? string.Empty,
            Name = thingsboardResponse.Name,
            Type = thingsboardResponse.Type,
            CreatedTime = thingsboardResponse.CreatedTime,
            Credentials = thingsboardResponse.Credentials != null
                ? new TbCredentials
                {
                    CredentialsType = thingsboardResponse.Credentials.CredentialsType,
                    CredentialsId = thingsboardResponse.Credentials.CredentialsId
                }
                : null
        };
    }

    /// <inheritdoc />
    public async Task<TbDeviceResponse?> GetDeviceByIdAsync(
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        var token = await GetValidTokenAsync(cancellationToken);

        var url = $"/api/device/{deviceId}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Failed to get device from Thingsboard. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var thingsboardResponse = JsonConvert.DeserializeObject<ThingsboardDeviceResponse>(responseContent);

        if (thingsboardResponse == null)
        {
            return null;
        }

        return new TbDeviceResponse
        {
            Id = thingsboardResponse.Id?.Id ?? string.Empty,
            Name = thingsboardResponse.Name,
            Type = thingsboardResponse.Type,
            CreatedTime = thingsboardResponse.CreatedTime,
            Credentials = thingsboardResponse.Credentials != null
                ? new TbCredentials
                {
                    CredentialsType = thingsboardResponse.Credentials.CredentialsType,
                    CredentialsId = thingsboardResponse.Credentials.CredentialsId
                }
                : null
        };
    }

    /// <inheritdoc />
    public async Task<bool> DeleteDeviceAsync(
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        var token = await GetValidTokenAsync(cancellationToken);

        var url = $"/api/device/{deviceId}";

        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Failed to delete device from Thingsboard. Status: {response.StatusCode}, Error: {errorContent}");
        }

        return true;
    }

    /// <inheritdoc />
    public async Task<TbCredentials?> GetDeviceCredentialsAsync(
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        var token = await GetValidTokenAsync(cancellationToken);

        var url = $"/api/device/{deviceId}/credentials";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Failed to get device credentials from Thingsboard. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var credentials = JsonConvert.DeserializeObject<DeviceCredentials>(responseContent);

        if (credentials == null)
        {
            return null;
        }

        return new TbCredentials
        {
            CredentialsType = credentials.CredentialsType,
            CredentialsId = credentials.CredentialsId
        };
    }

    /// <inheritdoc />
    public async Task<TbDeviceResponse?> GetDeviceByNameAsync(
        string deviceName,
        CancellationToken cancellationToken = default)
    {
        var token = await GetValidTokenAsync(cancellationToken);

        // Use the device name to search
        var url = $"/api/device?name={Uri.EscapeDataString(deviceName)}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Failed to get device by name from Thingsboard. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var thingsboardResponse = JsonConvert.DeserializeObject<ThingsboardDeviceResponse>(responseContent);

        if (thingsboardResponse == null)
        {
            return null;
        }

        return new TbDeviceResponse
        {
            Id = thingsboardResponse.Id?.Id ?? string.Empty,
            Name = thingsboardResponse.Name,
            Type = thingsboardResponse.Type,
            CreatedTime = thingsboardResponse.CreatedTime,
            Credentials = thingsboardResponse.Credentials != null
                ? new TbCredentials
                {
                    CredentialsType = thingsboardResponse.Credentials.CredentialsType,
                    CredentialsId = thingsboardResponse.Credentials.CredentialsId
                }
                : null
        };
    }

    /// <inheritdoc />
    public async Task<TbDeviceResponse> UpdateDeviceAsync(
        string deviceId,
        string deviceName,
        string deviceType,
        string? label = null,
        CancellationToken cancellationToken = default)
    {
        var token = await GetValidTokenAsync(cancellationToken);

        // Thingsboard uses POST /api/device for both create and update
        // When ID is in the body as an object, it updates the existing device
        var url = "/api/device";

        var device = new
        {
            id = new { entityType = "DEVICE", id = deviceId },
            name = deviceName,
            type = deviceType,
            label = label
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(
                JsonConvert.SerializeObject(device),
                Encoding.UTF8,
                "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Failed to update device in Thingsboard. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var thingsboardResponse = JsonConvert.DeserializeObject<ThingsboardDeviceResponse>(responseContent)
                                ?? throw new InvalidOperationException("Failed to deserialize Thingsboard response");

        return new TbDeviceResponse
        {
            Id = thingsboardResponse.Id?.Id ?? string.Empty,
            Name = thingsboardResponse.Name,
            Type = thingsboardResponse.Type,
            CreatedTime = thingsboardResponse.CreatedTime,
            Credentials = thingsboardResponse.Credentials != null
                ? new TbCredentials
                {
                    CredentialsType = thingsboardResponse.Credentials.CredentialsType,
                    CredentialsId = thingsboardResponse.Credentials.CredentialsId
                }
                : null
        };
    }

    private async Task<string> GetValidTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiration)
        {
            return _cachedToken;
        }

        return await AuthenticateAsync(cancellationToken);
    }

    private async Task<string> AuthenticateAsync(CancellationToken cancellationToken)
    {
        var authPayload = new
        {
            username = _options.TenantUsername,
            password = _options.TenantPassword
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(authPayload),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("/api/auth/login", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Failed to authenticate with Thingsboard. Status: {response.StatusCode}, Error: {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var authResponse = JsonConvert.DeserializeObject<ThingsboardAuthResponse>(responseContent)
                          ?? throw new InvalidOperationException("Failed to deserialize Thingsboard auth response");

        _cachedToken = authResponse.Token;
        _tokenExpiration = DateTime.UtcNow.AddMinutes(_options.TokenExpirationMinutes);

        return _cachedToken;
    }
}

/// <summary>
/// Device entity for Thingsboard API (internal)
/// </summary>
internal class ThingsboardDeviceResponse
{
    [JsonProperty("id")]
    public ThingsboardEntityId? Id { get; set; }

    [JsonProperty("tenantId")]
    public ThingsboardEntityId? TenantId { get; set; }

    [JsonProperty("customerId")]
    public ThingsboardEntityId? CustomerId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("type")]
    public string? Type { get; set; }

    [JsonProperty("label")]
    public string? Label { get; set; }

    [JsonProperty("deviceProfileId")]
    public ThingsboardEntityId? DeviceProfileId { get; set; }

    [JsonProperty("firmwareId")]
    public ThingsboardEntityId? FirmwareId { get; set; }

    [JsonProperty("softwareId")]
    public ThingsboardEntityId? SoftwareId { get; set; }

    [JsonProperty("additionalInfo")]
    public Dictionary<string, object>? AdditionalInfo { get; set; }

    [JsonProperty("createdTime")]
    public long? CreatedTime { get; set; }

    [JsonProperty("credentials")]
    public DeviceCredentials? Credentials { get; set; }
}

/// <summary>
/// Thingsboard entity ID wrapper (internal)
/// </summary>
internal class ThingsboardEntityId
{
    [JsonProperty("entityType")]
    public string? EntityType { get; set; }

    [JsonProperty("id")]
    public string? Id { get; set; }
}

/// <summary>
/// Device credentials from Thingsboard (internal)
/// </summary>
internal class DeviceCredentials
{
    [JsonProperty("id")]
    public ThingsboardEntityId? Id { get; set; }

    [JsonProperty("deviceId")]
    public ThingsboardEntityId? DeviceId { get; set; }

    [JsonProperty("credentialsType")]
    public string? CredentialsType { get; set; }

    [JsonProperty("credentialsId")]
    public string? CredentialsId { get; set; }
}

/// <summary>
/// Thingsboard authentication response (internal)
/// </summary>
internal class ThingsboardAuthResponse
{
    [JsonProperty("token")]
    public string Token { get; set; } = string.Empty;

    [JsonProperty("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonProperty("tokenExpiresIn")]
    public long TokenExpiresIn { get; set; }

    [JsonProperty("refreshTokenExpiresIn")]
    public long RefreshTokenExpiresIn { get; set; }
}
