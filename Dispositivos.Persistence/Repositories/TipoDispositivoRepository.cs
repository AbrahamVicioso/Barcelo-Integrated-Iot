using Dispositivos.Application.Interfaces;
using Dispositivos.Domain.Entities;
using Dispositivos.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Dispositivos.Persistence.Repositories;

public class TipoDispositivoRepository : ITipoDispositivoRepository
{
    private readonly BarceloIoTDatabaseContext _context;

    public TipoDispositivoRepository(BarceloIoTDatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TipoDispositivo>> GetAll()
        => await _context.TiposDispositivo.AsNoTracking().ToListAsync();

    public async Task<TipoDispositivo?> GetById(int id)
        => await _context.TiposDispositivo.AsNoTracking().FirstOrDefaultAsync(t => t.TipoDispositivoId == id);

    public async Task AddAsync(TipoDispositivo tipo, CancellationToken cancellationToken = default)
        => await _context.TiposDispositivo.AddAsync(tipo, cancellationToken);

    public async Task UpdateAsync(TipoDispositivo tipo, CancellationToken cancellationToken = default)
    {
        _context.TiposDispositivo.Update(tipo);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(TipoDispositivo tipo, CancellationToken cancellationToken = default)
    {
        _context.TiposDispositivo.Remove(tipo);
        await Task.CompletedTask;
    }
}
