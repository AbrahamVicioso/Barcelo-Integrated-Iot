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
}
