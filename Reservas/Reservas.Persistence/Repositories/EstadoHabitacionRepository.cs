using Microsoft.EntityFrameworkCore;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entities;
using Reservas.Persistence.Data;

namespace Reservas.Persistence.Repositories;

public class EstadoHabitacionRepository : IEstadoHabitacionRepository
{
    private readonly BarceloReservasContext _context;

    public EstadoHabitacionRepository(BarceloReservasContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EstadoHabitacion>> GetAll()
    {
        return await _context.EstadosHabitacion.AsNoTracking().ToListAsync();
    }

    public async Task<EstadoHabitacion?> GetById(int id)
    {
        return await _context.EstadosHabitacion.AsNoTracking().FirstOrDefaultAsync(e => e.EstadoHabitacionId == id);
    }

    public async Task AddAsync(EstadoHabitacion estadoHabitacion, CancellationToken cancellationToken = default)
    {
        await _context.EstadosHabitacion.AddAsync(estadoHabitacion, cancellationToken);
    }

    public async Task UpdateAsync(EstadoHabitacion estadoHabitacion, CancellationToken cancellationToken = default)
    {
        _context.EstadosHabitacion.Update(estadoHabitacion);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(EstadoHabitacion estadoHabitacion, CancellationToken cancellationToken = default)
    {
        _context.EstadosHabitacion.Remove(estadoHabitacion);
        await Task.CompletedTask;
    }
}
