using Microsoft.EntityFrameworkCore;
using Reservas.Domain.Entities;
using Reservas.Application.Interfaces;
using Reservas.Persistence.Data;

namespace Reservas.Persistence.Repositories;

public class HotelRepository : IHotelRepository
{
    private readonly BarceloReservasContext _context;

    public HotelRepository(BarceloReservasContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Hotel>> GetAll()
    {
        return await _context.Hoteles.ToListAsync();
    }

    public async Task<Hotel?> GetById(int id)
    {
        return await _context.Hoteles.FindAsync(id);
    }

    public async Task AddAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        await _context.Hoteles.AddAsync(hotel, cancellationToken);
    }

    public async Task UpdateAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        _context.Hoteles.Update(hotel);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        _context.Hoteles.Remove(hotel);
        await Task.CompletedTask;
    }
}
