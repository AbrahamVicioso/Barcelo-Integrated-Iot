using Microsoft.EntityFrameworkCore;
using Reservas.Domain.Entites;
using Reservas.Domain.Interfaces;
using Reservas.Persistence.Data;

namespace Reservas.Persistence.Repositories;

public class ReservaRepository : GenericRepository<Reserva>, IReservaRepository
{
    public ReservaRepository(BarceloReservasContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Reserva>> GetReservasByHuespedIdAsync(int huespedId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.HuespedId == huespedId)
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<Reserva?> GetByNumeroReservaAsync(string numeroReserva, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.NumeroReserva == numeroReserva, cancellationToken);
    }

    public async Task<IEnumerable<Reserva>> GetReservasByEstadoAsync(string estado, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.Estado == estado)
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
}
