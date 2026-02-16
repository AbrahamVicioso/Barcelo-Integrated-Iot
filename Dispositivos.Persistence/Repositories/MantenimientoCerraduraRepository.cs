using Dispositivos.Domain.Entities;
using Dispositivos.Application.Interfaces;
using Dispositivos.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Dispositivos.Persistence.Repositories;

public class MantenimientoCerraduraRepository : IMantenimientoCerraduraRepository
{
    private readonly BarceloIoTDatabaseContext _context;

    public MantenimientoCerraduraRepository(BarceloIoTDatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MantenimientoCerradura>> GetAll()
    {
        return await _context.MantenimientoCerraduras
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<MantenimientoCerradura?> GetById(int id)
    {
        return await _context.MantenimientoCerraduras
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.MantenimientoId == id);
    }

    public async Task<IEnumerable<MantenimientoCerradura>> GetByCerraduraId(int cerraduraId)
    {
        return await _context.MantenimientoCerraduras
            .Where(m => m.CerraduraId == cerraduraId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<MantenimientoCerradura>> GetByDispositivoId(Guid dispositivoId)
    {
        return await _context.MantenimientoCerraduras
            .Where(m => m.DispositivoId == dispositivoId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<MantenimientoCerradura>> GetByEstado(string estado)
    {
        return await _context.MantenimientoCerraduras
            .Where(m => m.Estado == estado)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<MantenimientoCerradura>> GetByPersonalId(int personalId)
    {
        return await _context.MantenimientoCerraduras
            .Where(m => m.PersonalId == personalId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(MantenimientoCerradura mantenimiento, CancellationToken cancellationToken = default)
    {
        await _context.MantenimientoCerraduras.AddAsync(mantenimiento, cancellationToken);
    }

    public async Task UpdateAsync(MantenimientoCerradura mantenimiento, CancellationToken cancellationToken = default)
    {
        _context.MantenimientoCerraduras.Update(mantenimiento);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(MantenimientoCerradura mantenimiento, CancellationToken cancellationToken = default)
    {
        _context.MantenimientoCerraduras.Remove(mantenimiento);
        await Task.CompletedTask;
    }
}
