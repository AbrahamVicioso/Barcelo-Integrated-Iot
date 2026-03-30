namespace Authentication.Api.DTOs;

public record PushConfigResponse(
    string Topic,
    string NtfyBaseUrl,
    string NtfyToken,
    /// <summary>Only present for Admin users. Subscribe to this topic for system-wide notifications.</summary>
    string? SystemTopic = null);
