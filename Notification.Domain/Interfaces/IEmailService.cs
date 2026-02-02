using Notification.Domain.Entities;

namespace Notification.Domain.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailNotification notification, CancellationToken cancellationToken = default);
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default);
    }
}
