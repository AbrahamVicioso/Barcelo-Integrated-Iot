using System.Net.Http.Json;
using Grpc.Contracts.Authentication;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Notification.Kafka.Services;

public class AuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthApiClient> _logger;

    public AuthApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<AuthApiClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
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
            _logger.LogWarning("[AuthGrpc] GetUserIdByEmailAsync called with empty email");
            return null;
        }

        var grpcUrl = _configuration["ExternalServices:Authenticate:GrpcUrl"] ?? "https://auth-api:5118";
        var skipCert = _configuration.GetValue<bool>("ExternalServices:Authenticate:SkipCertValidation");

        var httpHandler = new System.Net.Http.HttpClientHandler();
        if (skipCert)
            httpHandler.ServerCertificateCustomValidationCallback =
                System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(grpcUrl, new GrpcChannelOptions
                {
                    HttpHandler = httpHandler,
                    DisposeHttpClient = attempt == 3
                });
                var client = new UserLookup.UserLookupClient(channel);

                var response = await client.GetUserIdByEmailAsync(
                    new GetUserIdByEmailRequest { Email = email },
                    cancellationToken: cancellationToken);

                if (!response.Found)
                {
                    _logger.LogWarning("[AuthGrpc] Usuario no encontrado para email {Email}", email);
                    return null;
                }

                _logger.LogDebug("[AuthGrpc] UserId {UserId} obtenido para email {Email}", response.UserId, email);
                return response.UserId;
            }
            catch (Exception ex) when (attempt < 3)
            {
                _logger.LogWarning(ex, "[AuthGrpc] Error gRPC (attempt {Attempt}/3): {Message}", attempt, ex.Message);
                await Task.Delay(500 * attempt, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthGrpc] Error gRPC final para email {Email}: {Message}", email, ex.Message);
            }
        }

        return null;
    }
}
