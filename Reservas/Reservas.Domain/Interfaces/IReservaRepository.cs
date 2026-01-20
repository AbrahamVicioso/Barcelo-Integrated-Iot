using Reservas.Domain.Entites;

namespace Reservas.Domain.Interfaces;

public interface IReservaRepository : IGenericRepository<Reserva>
{
    Task<IEnumerable<Reserva>> GetReservasByHuespedIdAsync(int huespedId, CancellationToken cancellationToken = default);
    Task<Reserva?> GetByNumeroReservaAsync(string numeroReserva, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reserva>> GetReservasByEstadoAsync(string estado, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reserva>> GetReservasByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default);
}
