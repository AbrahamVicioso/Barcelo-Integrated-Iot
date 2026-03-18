using Notification.Domain.Events;

namespace Reservas.Application.Interfaces
{
    public interface IReservaKafkaProducer
    {
        Task PublishReservaCreadaAsync(ReservaCreadaEvent reservaEvent, CancellationToken cancellationToken = default);
        Task PublishUnlockDoorAsync(UnlockDoorEvent unlockDoorEvent, CancellationToken cancellationToken = default);
    }
}
