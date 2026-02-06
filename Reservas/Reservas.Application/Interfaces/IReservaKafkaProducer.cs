using Notification.Domain.Events;

namespace Reservas.Application.Interfaces
{
    public interface IReservaKafkaProducer
    {
        Task PublishReservaCreadaAsync(ReservaCreadaEvent reservaEvent, CancellationToken cancellationToken = default);
    }
}
