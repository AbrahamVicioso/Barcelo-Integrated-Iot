using Microsoft.EntityFrameworkCore;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entites;
using Reservas.Persistence.Data;

namespace Reservas.Persistence.Repositories;

public class EstadoReservaRepository : IEstadoReservaRepository
{
    private readonly BarceloReservasContext _context;

    public EstadoReservaRepository(BarceloReservasContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EstadoReserva>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EstadosReserva.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<EstadoReserva?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.EstadosReserva.AsNoTracking().FirstOrDefaultAsync(e => e.EstadoReservaId == id, cancellationToken);
    }
}
