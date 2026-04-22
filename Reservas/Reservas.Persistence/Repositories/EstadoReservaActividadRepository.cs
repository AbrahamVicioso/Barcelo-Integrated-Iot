using Microsoft.EntityFrameworkCore;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entites;
using Reservas.Persistence.Data;

namespace Reservas.Persistence.Repositories;

public class EstadoReservaActividadRepository : IEstadoReservaActividadRepository
{
    private readonly BarceloReservasContext _context;

    public EstadoReservaActividadRepository(BarceloReservasContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EstadoReservaActividad>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EstadosReservaActividad.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<EstadoReservaActividad?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.EstadosReservaActividad.AsNoTracking().FirstOrDefaultAsync(e => e.EstadoReservaActividadId == id, cancellationToken);
    }

    public async Task AddAsync(EstadoReservaActividad entity, CancellationToken cancellationToken = default)
    {
        await _context.EstadosReservaActividad.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(EstadoReservaActividad entity, CancellationToken cancellationToken = default)
    {
        _context.EstadosReservaActividad.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(EstadoReservaActividad entity, CancellationToken cancellationToken = default)
    {
        _context.EstadosReservaActividad.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}