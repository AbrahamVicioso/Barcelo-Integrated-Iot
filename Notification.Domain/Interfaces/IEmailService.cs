using Notification.Domain.Entities;

namespace Notification.Domain.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailNotification notification, CancellationToken cancellationToken = default);
    }
}
