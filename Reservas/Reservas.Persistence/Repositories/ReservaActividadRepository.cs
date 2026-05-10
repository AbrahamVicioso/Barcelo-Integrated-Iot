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

    public override async Task<IEnumerable<ReservasActividades>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync(cancellationToken);
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

    public async Task<ReservasActividades?> GetByIdWithActividadAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Actividad)
            .FirstOrDefaultAsync(r => r.ReservaActividadId == id, cancellationToken);
    }

    public async Task<IEnumerable<ReservasActividades>> GetProximasParaRecordatorioAsync(int minutosAntes, CancellationToken cancellationToken = default)
    {
        var ahora = DateTime.Now;
        var limite = ahora.AddMinutes(minutosAntes);

        // EF Core cannot translate DateTime.Add(TimeSpan) to SQL.
        // Filter by date in SQL, then apply time-window filter in memory.
        var candidatas = await _dbSet
            .Include(r => r.Actividad)
            .Where(r =>
                !r.RecordatorioEnviado &&
                r.Estado != "Cancelada" &&
                r.FechaReserva.Date == ahora.Date)
            .ToListAsync(cancellationToken);

        return candidatas
            .Where(r =>
            {
                var inicio = r.FechaReserva.Date.Add(r.HoraReserva);
                return inicio > ahora && inicio <= limite;
            })
            .ToList();
    }
}
