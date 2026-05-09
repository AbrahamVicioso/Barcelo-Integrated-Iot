namespace Notification.Domain.Events
{
    public class TwoFactorCodeEvent
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Provider { get; set; } = "Email";
        public int ExpirationMinutes { get; set; } = 5;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}