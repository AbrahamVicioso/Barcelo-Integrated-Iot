using Microsoft.EntityFrameworkCore;
using Reservas.Domain.Entites;
using Reservas.Application.Interfaces;
using Reservas.Persistence.Data;

namespace Reservas.Persistence.Repositories;

public class ReservaActividadRepository : GenericRepository<ReservasActividades>, IReservaActividadRepository
{
    public ReservaActividadRepository(BarceloReservasContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ReservasActividades>> GetReservasByHuespedIdAsync(int huespedId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Actividad)
            .Where(r => r.HuespedId == huespedId)
            .OrderByDescending(r => r.FechaReserva)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ReservasActividades>> GetReservasByActividadIdAsync(int actividadId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Actividad)
            .Where(r => r.ActividadId == actividadId)
            .OrderByDescending(r => r.FechaReserva)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ReservasActividades>> GetReservasByFechaAsync(DateTime fecha, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Actividad)
            .Where(r => r.FechaReserva.Date == fecha.Date)
            .OrderBy(r => r.HoraReserva)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ReservasActividades>> GetReservasByEstadoAsync(string estado, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Actividad)
            .Where(r => r.Estado == estado)
            .OrderByDescending(r => r.FechaReserva)
            .ToListAsync(cancellationToken);
    }
}
