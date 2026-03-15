using Dispositivos.Application.Interfaces;
using Dispositivos.Domain.Entities;
using Dispositivos.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Dispositivos.Persistence.Repositories;

public class EstadoDispositivoRepository : IEstadoDispositivoRepository
{
    private readonly BarceloIoTDatabaseContext _context;

    public EstadoDispositivoRepository(BarceloIoTDatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EstadoDispositivo>> GetAll()
    {
        return await _context.EstadosDispositivo.AsNoTracking().ToListAsync();
    }

    public async Task<EstadoDispositivo?> GetById(int id)
    {
        return await _context.EstadosDispositivo.AsNoTracking().FirstOrDefaultAsync(e => e.EstadoDispositivoId == id);
    }

    public async Task AddAsync(EstadoDispositivo estadoDispositivo, CancellationToken cancellationToken = default)
    {
        await _context.EstadosDispositivo.AddAsync(estadoDispositivo, cancellationToken);
    }

    public async Task UpdateAsync(EstadoDispositivo estadoDispositivo, CancellationToken cancellationToken = default)
    {
        _context.EstadosDispositivo.Update(estadoDispositivo);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(EstadoDispositivo estadoDispositivo, CancellationToken cancellationToken = default)
    {
        _context.EstadosDispositivo.Remove(estadoDispositivo);
        await Task.CompletedTask;
    }
}
