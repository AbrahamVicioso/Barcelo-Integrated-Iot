using Notification.Domain.Entities;

namespace Notification.Domain.Interfaces;

public interface IPushNotificationService
{
    Task<bool> SendAsync(PushNotification notification, CancellationToken cancellationToken = default);
}
