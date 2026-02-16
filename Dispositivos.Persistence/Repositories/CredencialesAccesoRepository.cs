using Dispositivos.Domain.Entities;
using Dispositivos.Application.Interfaces;
using Dispositivos.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Dispositivos.Persistence.Repositories;

public class CredencialesAccesoRepository : ICredencialesAccesoRepository
{
    private readonly BarceloIoTDatabaseContext _context;

    public CredencialesAccesoRepository(BarceloIoTDatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CredencialesAcceso>> GetAll()
    {
        return await _context.CredencialesAccesos
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<CredencialesAcceso?> GetById(int id)
    {
        return await _context.CredencialesAccesos
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CredencialId == id);
    }

    public async Task<IEnumerable<CredencialesAcceso>> GetByHuespedId(int huespedId)
    {
        return await _context.CredencialesAccesos
            .Where(c => c.HuespedId == huespedId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<CredencialesAcceso>> GetByPersonalId(int personalId)
    {
        return await _context.CredencialesAccesos
            .Where(c => c.PersonalId == personalId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<CredencialesAcceso>> GetByEstaActiva(bool estaActiva)
    {
        return await _context.CredencialesAccesos
            .Where(c => c.EstaActiva == estaActiva)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<CredencialesAcceso>> GetByTipoCredencial(string tipoCredencial)
    {
        return await _context.CredencialesAccesos
            .Where(c => c.TipoCredencial == tipoCredencial)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<CredencialesAcceso?> GetByCodigoPin(string codigoPin)
    {
        return await _context.CredencialesAccesos
            .Where(c => c.CodigoPin == codigoPin)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(CredencialesAcceso credencial, CancellationToken cancellationToken = default)
    {
        await _context.CredencialesAccesos.AddAsync(credencial, cancellationToken);
    }

    public async Task UpdateAsync(CredencialesAcceso credencial, CancellationToken cancellationToken = default)
    {
        _context.CredencialesAccesos.Update(credencial);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(CredencialesAcceso credencial, CancellationToken cancellationToken = default)
    {
        _context.CredencialesAccesos.Remove(credencial);
        await Task.CompletedTask;
    }
}
