using Reservas.Domain.Entites;

namespace Reservas.Application.Interfaces;

public interface IReservaRepository : IGenericRepository<Reserva>
{
    Task<IEnumerable<Reserva>> GetReservasByHuespedIdAsync(int huespedId, CancellationToken cancellationToken = default);
    Task<Reserva?> GetByNumeroReservaAsync(string numeroReserva, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reserva>> GetReservasByEstadoAsync(int estadoReservaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reserva>> GetReservasByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
    Task<bool> IsHabitacionOcupadaAsync(int habitacionId, DateTime fechaCheckIn, DateTime fechaCheckOut, CancellationToken cancellationToken = default, int? excludeReservaId = null);
}
