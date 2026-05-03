using Notification.Domain.Events;

namespace Reservas.Application.Interfaces;

public interface IReservaActividadKafkaProducer
{
    Task PublishReservaActividadConfirmadaAsync(ReservaActividadConfirmadaEvent evt, CancellationToken cancellationToken = default);
    Task PublishActividadUnlockDoorAsync(ActividadUnlockDoorEvent unlockEvent, CancellationToken cancellationToken = default);
    Task PublishPersonalActividadUnlockDoorAsync(PersonalActividadUnlockDoorEvent unlockEvent, CancellationToken cancellationToken = default);
}
