using Reservas.Domain.Entites;

namespace Reservas.Application.Interfaces;

public interface IReservaActividadRepository : IGenericRepository<ReservasActividades>
{
    Task<IEnumerable<ReservasActividades>> GetReservasByHuespedIdAsync(int huespedId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReservasActividades>> GetReservasByActividadIdAsync(int actividadId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReservasActividades>> GetReservasByFechaAsync(DateTime fecha, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReservasActividades>> GetReservasByEstadoAsync(string estado, CancellationToken cancellationToken = default);
    Task<ReservasActividades?> GetByIdWithActividadAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ReservasActividades>> GetProximasParaRecordatorioAsync(int minutosAntes, CancellationToken cancellationToken = default);
}
