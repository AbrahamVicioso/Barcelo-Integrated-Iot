namespace Notification.Domain.Helpers;

public static class NtfyTopicHelper
{
    /// <summary>
    /// Derives a URL-safe ntfy topic name from a user's email.
    /// e.g. usuario@hotel.com → barcelo-usuario-at-hotel-com
    /// </summary>
    public static string GetUserTopic(string email) =>
        "barcelo-" + email.ToLower()
            .Replace("@", "-at-")
            .Replace(".", "-")
            .Replace("+", "-")
            .Replace("_", "-");
}
