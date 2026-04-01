namespace Notification.Push;

public class NtfyOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8081";

    /// <summary>Server token (Bearer) used by the Notification.Worker to publish messages. Must have write access.</summary>
    public string? AccessToken { get; set; }

    /// <summary>Admin password for the ntfy admin user. Used to create per-user accounts via the admin API.</summary>
    public string? AdminPassword { get; set; }

    public string TopicPrefix { get; set; } = "barcelo";

    /// <summary>Fixed topic for system-wide admin notifications.</summary>
    public string SystemTopic { get; set; } = "barcelo-system";

    /// <summary>Skip TLS certificate validation. Use only in development with self-signed certs.</summary>
    public bool IgnoreCertificateErrors { get; set; } = false;

    /// <summary>
    /// Publicly accessible ntfy URL returned to clients in push-config.
    /// Should point to the API Gateway ntfy route (e.g. http://localhost:5019/ntfy).
    /// Falls back to BaseUrl if not set.
    /// </summary>
    public string? PublicBaseUrl { get; set; }
}
