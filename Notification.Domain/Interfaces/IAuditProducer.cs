using Notification.Domain.Events;

namespace Notification.Domain.Interfaces
{
    public interface IAuditProducer
    {
        Task PublishAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
    }
}
