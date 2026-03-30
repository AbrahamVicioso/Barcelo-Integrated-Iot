namespace Notification.Domain.Interfaces;

public interface INtfyAdminService
{
    /// <summary>
    /// Creates a ntfy user account with read-only access to their personal topic.
    /// Returns a long-lived ntfy access token (tk_...) — the underlying password is
    /// never exposed. Returns null if account or token creation fails.
    /// </summary>
    Task<string?> CreateUserAccountAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Grants read-only access to the system topic for the given ntfy username.
    /// Idempotent — safe to call multiple times.
    /// </summary>
    Task GrantSystemTopicAccessAsync(string ntfyUsername, CancellationToken cancellationToken = default);
}
