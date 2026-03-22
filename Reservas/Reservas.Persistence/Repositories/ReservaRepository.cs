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
            .FirstOrDefaultAsync(r => r.ReservaId == id, cancellationToken);
    }

    public override async Task<IEnumerable<Reserva>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.EstadoReserva)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reserva>> GetReservasByHuespedIdAsync(int huespedId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.EstadoReserva)
            .Where(r => r.HuespedId == huespedId)
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<Reserva?> GetByNumeroReservaAsync(string numeroReserva, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.EstadoReserva)
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
}
