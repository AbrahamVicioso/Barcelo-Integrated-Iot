namespace Notification.Domain.Helpers;

public static class NtfyTopicHelper
{
    /// <summary>
    /// Derives a ntfy topic/username from a user's email using only [a-z0-9].
    /// ntfy rejects topic names with hyphens or underscores in ACL operations on some builds.
    /// e.g. usuario@hotel.com → barcelousuarioathotelcom
    /// </summary>
    public static string GetUserTopic(string email)
    {
        var normalized = email.ToLower()
            .Replace("@", "at")
            .Replace(".", "");

        return "barcelo" + new string(normalized.Where(c => c is >= 'a' and <= 'z' or >= '0' and <= '9').ToArray());
    }
}
