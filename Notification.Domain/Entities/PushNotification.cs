namespace Notification.Domain.Entities;

public class PushNotification
{
    public string Topic { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public PushPriority Priority { get; set; } = PushPriority.Default;
    public string[]? Tags { get; set; }
    public string? ClickUrl { get; set; }
}

public enum PushPriority
{
    Min = 1,
    Low = 2,
    Default = 3,
    High = 4,
    Urgent = 5
}
