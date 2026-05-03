using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Dispositivos.Application.Interfaces;
using Dispositivos.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Dispositivos.Infrastructure.Services;

/// <summary>
/// Singleton token cache shared across all TbDeviceService instances
/// </summary>
public class TbTokenCache
{
    private string? _token;
    private DateTime _expiration = DateTime.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public bool TryGet(out string? token)
    {
        token = _token;
        return !string.IsNullOrEmpty(_token) && DateTime.UtcNow < _expiration;
    }

    public async Task<string> GetOrRefreshAsync(
        Func<CancellationToken, Task<(string token, int expirationMinutes)>> factory,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_token) && DateTime.UtcNow < _expiration)
            return _token!;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (!string.IsNullOrEmpty(_token) && DateTime.UtcNow < _expiration)
                return _token!;

            var (newToken, minutes) = await factory(cancellationToken);
            _token = newToken;
            _expiration = DateTime.UtcNow.AddMinutes(minutes);
            return _token;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Invalidate()
    {
        _token = null;
        _expiration = DateTime.MinValue;
    }
}

/// <summary>
/// Implementation of Thingsboard device management service using HTTP API
/// </summary>
public class TbDeviceService : ITbDeviceService
{
    private readonly HttpClient _httpClient;
    private readonly ThingsboardOptions _options;
    private readonly TbTokenCache _tokenCache;

    public TbDeviceService(
        HttpClient httpClient,
        IOptions<ThingsboardOptions> options,
        TbTokenCache tokenCache)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _tokenCache = tokenCache;
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
            queryParams.Add($"accessToken={Uri.EscapeDataString(accessToken)}");

        if (!string.IsNullOrEmpty(_options.NameConflictPolicy))
            queryParams.Add($"nameConflictPolicy={Uri.EscapeDataString(_options.NameConflictPolicy)}");

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

            // Si ThingsBoard dice que el device ya existe, recuperarlo por nombre (operación idempotente)
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest
                && errorContent.Contains("already exists"))
            {
                var existing = await GetDeviceByNameAsync(deviceName, cancellationToken);
                if (existing != null)
                    return existing;
            }

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

        // ThingsBoard tenant endpoint to find a device by exact name
        var url = $"/api/tenant/devices?deviceName={Uri.EscapeDataString(deviceName)}";

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

    /// <inheritdoc />
    public async Task SetSharedAttributesAsync(
        string deviceId,
        Dictionary<string, object> attributes,
        CancellationToken cancellationToken = default)
    {
        var token = await GetValidTokenAsync(cancellationToken);

        var url = $"/api/plugins/telemetry/DEVICE/{deviceId}/SHARED_SCOPE";

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(
                JsonConvert.SerializeObject(attributes),
                Encoding.UTF8,
                "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Failed to set shared attributes on device {deviceId} in Thingsboard. Status: {response.StatusCode}, Error: {errorContent}");
        }
    }

    /// <inheritdoc />
    public async Task SendTelemetryAsync(
        string deviceId,
        Dictionary<string, object> telemetry,
        CancellationToken cancellationToken = default)
    {
        var token = await GetValidTokenAsync(cancellationToken);

        // Server-side one-way RPC: device processes once, does not persist as state.
        // Device must subscribe to v1/devices/me/rpc/request/+ (MQTT) or poll HTTP.
        var url = $"/api/rpc/oneway/{deviceId}";

        var body = new
        {
            method = "unlock",
            @params = telemetry
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(
                JsonConvert.SerializeObject(body),
                Encoding.UTF8,
                "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Failed to send RPC unlock to device {deviceId} in Thingsboard. Status: {response.StatusCode}, Error: {errorContent}");
        }
    }

    private Task<string> GetValidTokenAsync(CancellationToken cancellationToken)
    {
        return _tokenCache.GetOrRefreshAsync(AuthenticateAsync, cancellationToken);
    }

    private async Task<(string token, int expirationMinutes)> AuthenticateAsync(CancellationToken cancellationToken)
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

        return (authResponse.Token, _options.TokenExpirationMinutes);
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
