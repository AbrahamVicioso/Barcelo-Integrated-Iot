using Notification.Domain.Events;

namespace Reservas.Application.Interfaces
{
    public interface IReservaKafkaProducer
    {
        Task PublishReservaCreadaAsync(ReservaCreadaEvent reservaEvent, CancellationToken cancellationToken = default);
        Task PublishUnlockDoorAsync(UnlockDoorEvent unlockDoorEvent, CancellationToken cancellationToken = default);
        Task PublishCheckInRealizadoAsync(CheckInRealizadoEvent checkInEvent, CancellationToken cancellationToken = default);
        Task PublishPersonalUnlockDoorAsync(PersonalUnlockDoorEvent personalUnlockEvent, CancellationToken cancellationToken = default);
        /// <summary>
        /// Notifica a Dispositivos que recalcule las credenciales ThingsBoard de una habitación.
        /// Usar cuando cambia la habitación de una reserva con check-in activo.
        /// </summary>
        Task PublishHabitacionSyncAsync(int habitacionId, CancellationToken cancellationToken = default);
    }
}
