using Notification.Domain.Events;

namespace Reservas.Application.Interfaces
{
    public interface IReservaKafkaProducer
    {
        Task PublishReservaCreadaAsync(ReservaCreadaEvent reservaEvent, CancellationToken cancellationToken = default);
        Task PublishUnlockDoorAsync(UnlockDoorEvent unlockDoorEvent, CancellationToken cancellationToken = default);
        Task PublishCheckInRealizadoAsync(CheckInRealizadoEvent checkInEvent, CancellationToken cancellationToken = default);
        Task PublishPersonalUnlockDoorAsync(PersonalUnlockDoorEvent personalUnlockEvent, CancellationToken cancellationToken = default);
    }
}
