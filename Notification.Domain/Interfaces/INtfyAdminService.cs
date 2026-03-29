namespace Notification.Domain.Interfaces;

public interface INtfyAdminService
{
    /// <summary>
    /// Creates a ntfy user account with read-only access to their personal topic.
    /// Returns the generated ntfy password, or null if creation failed.
    /// </summary>
    Task<string?> CreateUserAccountAsync(string email, CancellationToken cancellationToken = default);
}
