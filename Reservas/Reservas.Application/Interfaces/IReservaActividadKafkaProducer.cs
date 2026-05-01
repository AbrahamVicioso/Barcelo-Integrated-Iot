using Notification.Domain.Events;

namespace Reservas.Application.Interfaces;

public interface IReservaActividadKafkaProducer
{
    Task PublishReservaActividadConfirmadaAsync(ReservaActividadConfirmadaEvent evt, CancellationToken cancellationToken = default);
}
