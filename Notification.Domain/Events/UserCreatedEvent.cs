namespace Notification.Domain.Events
{
    public class UserCreatedEvent
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string GeneratedPassword { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
