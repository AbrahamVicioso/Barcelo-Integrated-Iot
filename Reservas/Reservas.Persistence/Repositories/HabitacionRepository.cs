using Microsoft.EntityFrameworkCore;
using Reservas.Domain.Entities;
using Reservas.Application.Interfaces;
using Reservas.Persistence.Data;

namespace Reservas.Persistence.Repositories;

public class HabitacionRepository : IHabitacionRepository
{
    private readonly BarceloReservasContext _context;

    public HabitacionRepository(BarceloReservasContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Habitacion>> GetAll()
    {
        return await _context.Habitaciones
            .Include(h => h.Hotel)
            .ToListAsync();
    }

    public async Task<Habitacion?> GetById(int id)
    {
        return await _context.Habitaciones
            .Include(h => h.Hotel)
            .FirstOrDefaultAsync(h => h.HabitacionId == id);
    }

    public async Task<IEnumerable<Habitacion>> GetByHotelId(int hotelId)
    {
        return await _context.Habitaciones
            .Where(h => h.HotelId == hotelId)
            .Include(h => h.Hotel)
            .ToListAsync();
    }

    public async Task AddAsync(Habitacion habitacion, CancellationToken cancellationToken = default)
    {
        await _context.Habitaciones.AddAsync(habitacion, cancellationToken);
    }

    public async Task UpdateAsync(Habitacion habitacion, CancellationToken cancellationToken = default)
    {
        _context.Habitaciones.Update(habitacion);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Habitacion habitacion, CancellationToken cancellationToken = default)
    {
        _context.Habitaciones.Remove(habitacion);
        await Task.CompletedTask;
    }
}
