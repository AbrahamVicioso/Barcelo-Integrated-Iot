using Reservas.Domain.Entites;

namespace Reservas.Application.Interfaces;

public interface IReservaActividadRepository : IGenericRepository<ReservasActividade>
{
    Task<IEnumerable<ReservasActividade>> GetReservasByHuespedIdAsync(int huespedId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReservasActividade>> GetReservasByActividadIdAsync(int actividadId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReservasActividade>> GetReservasByFechaAsync(DateTime fecha, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReservasActividade>> GetReservasByEstadoAsync(string estado, CancellationToken cancellationToken = default);
}
