using Dispositivos.Domain.Entities;
using Dispositivos.Application.Interfaces;
using Dispositivos.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Dispositivos.Persistence.Repositories;

public class RegistrosAccesoRepository : IRegistrosAccesoRepository
{
    private readonly BarceloIoTDatabaseContext _context;

    public RegistrosAccesoRepository(BarceloIoTDatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RegistrosAcceso>> GetAllAsync()
    {
        return await _context.RegistrosAccesos
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<RegistrosAcceso?> GetByIdAsync(int id)
    {
        return await _context.RegistrosAccesos
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RegistroId == id);
    }

    public async Task<IEnumerable<RegistrosAcceso>> GetByCerraduraIdAsync(int cerraduraId)
    {
        return await _context.RegistrosAccesos
            .Where(r => r.CerraduraId == cerraduraId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<RegistrosAcceso>> GetByUsuarioIdAsync(string usuarioId)
    {
        return await _context.RegistrosAccesos
            .Where(r => r.UsuarioId == usuarioId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<RegistrosAcceso>> GetByFueExitosoAsync(bool fueExitoso)
    {
        return await _context.RegistrosAccesos
            .Where(r => r.FueExitoso == fueExitoso)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<RegistrosAcceso> AddAsync(RegistrosAcceso registro, CancellationToken cancellationToken = default)
    {
        await _context.RegistrosAccesos.AddAsync(registro, cancellationToken);
        return registro;
    }

    public async Task UpdateAsync(RegistrosAcceso registro, CancellationToken cancellationToken = default)
    {
        _context.RegistrosAccesos.Update(registro);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var registro = await _context.RegistrosAccesos.FindAsync(new object[] { id }, cancellationToken);
        if (registro != null)
        {
            _context.RegistrosAccesos.Remove(registro);
        }
    }
}
