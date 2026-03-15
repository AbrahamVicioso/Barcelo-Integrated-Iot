using Microsoft.EntityFrameworkCore;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entities;
using Reservas.Persistence.Data;

namespace Reservas.Persistence.Repositories;

public class TipoHabitacionRepository : ITipoHabitacionRepository
{
    private readonly BarceloReservasContext _context;

    public TipoHabitacionRepository(BarceloReservasContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TipoHabitacion>> GetAll()
    {
        return await _context.TiposHabitacion.AsNoTracking().ToListAsync();
    }

    public async Task<TipoHabitacion?> GetById(int id)
    {
        return await _context.TiposHabitacion.AsNoTracking().FirstOrDefaultAsync(t => t.TipoHabitacionId == id);
    }

    public async Task AddAsync(TipoHabitacion tipoHabitacion, CancellationToken cancellationToken = default)
    {
        await _context.TiposHabitacion.AddAsync(tipoHabitacion, cancellationToken);
    }

    public async Task UpdateAsync(TipoHabitacion tipoHabitacion, CancellationToken cancellationToken = default)
    {
        _context.TiposHabitacion.Update(tipoHabitacion);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(TipoHabitacion tipoHabitacion, CancellationToken cancellationToken = default)
    {
        _context.TiposHabitacion.Remove(tipoHabitacion);
        await Task.CompletedTask;
    }
}
