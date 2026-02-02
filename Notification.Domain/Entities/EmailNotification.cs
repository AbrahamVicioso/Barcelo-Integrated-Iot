namespace Notification.Domain.Entities
{
    public class EmailNotification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    }

    public enum NotificationStatus
    {
        Pending,
        Sent,
        Failed
    }
}
