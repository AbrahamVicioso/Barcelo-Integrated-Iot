using Microsoft.EntityFrameworkCore;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entites;

using Reservas.Persistence.Data;

namespace Reservas.Persistence.Repositories;

public class ReservaRepository : GenericRepository<Reserva>, IReservaRepository
{
    public ReservaRepository(BarceloReservasContext context) : base(context)
    {
    }

    public override async Task<Reserva?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.EstadoReserva)
            .Include(r => r.ReservaHuespedes)
            .FirstOrDefaultAsync(r => r.ReservaId == id, cancellationToken);
    }

    public override async Task<IEnumerable<Reserva>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.EstadoReserva)
            .Include(r => r.ReservaHuespedes)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reserva>> GetReservasByHuespedIdAsync(int huespedId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.EstadoReserva)
            .Include(r => r.ReservaHuespedes)
            .Where(r => r.HuespedId == huespedId || r.ReservaHuespedes.Any(rh => rh.HuespedId == huespedId))
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<Reserva?> GetByNumeroReservaAsync(string numeroReserva, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.EstadoReserva)
            .Include(r => r.ReservaHuespedes)
            .FirstOrDefaultAsync(r => r.NumeroReserva == numeroReserva, cancellationToken);
    }

    public async Task<IEnumerable<Reserva>> GetReservasByEstadoAsync(int estadoReservaId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.EstadoReservaId == estadoReservaId)
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reserva>> GetReservasByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.FechaCheckIn >= fechaInicio && r.FechaCheckOut <= fechaFin)
            .OrderBy(r => r.FechaCheckIn)
            .ToListAsync(cancellationToken);
    }

public async Task<bool> IsHabitacionOcupadaAsync(int habitacionId, DateTime fechaCheckIn, DateTime fechaCheckOut, CancellationToken cancellationToken = default, int? excludeReservaId = null)
{
    return await _dbSet.AnyAsync(r =>
        r.HabitacionId == habitacionId &&
        (r.EstadoReservaId != EstadoReserva.Cancelada || r.EstadoReservaId != EstadoReserva.CheckOut) &&
        r.FechaCheckIn < fechaCheckOut &&
        r.FechaCheckOut > fechaCheckIn &&
        (excludeReservaId == null || r.ReservaId != excludeReservaId),
        cancellationToken);
}

public async Task<Reserva?> GetReservaActivaByHabitacionIdAsync(int habitacionId, CancellationToken cancellationToken = default)
{
    var now = DateTime.UtcNow;
    return await _dbSet
        .Include(r => r.ReservaHuespedes)
        .FirstOrDefaultAsync(r =>
            r.HabitacionId == habitacionId &&
            r.EstadoReservaId == 2 &&
            r.FechaCheckIn <= now &&
            r.FechaCheckOut >= now,
            cancellationToken);
}
}
