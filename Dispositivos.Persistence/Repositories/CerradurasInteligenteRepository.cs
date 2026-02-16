using Dispositivos.Domain.Entities;
using Dispositivos.Application.Interfaces;
using Dispositivos.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Dispositivos.Persistence.Repositories;

public class CerradurasInteligenteRepository : ICerradurasInteligenteRepository
{
    private readonly BarceloIoTDatabaseContext _context;

    public CerradurasInteligenteRepository(BarceloIoTDatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CerradurasInteligente>> GetAll()
    {
        return await _context.CerradurasInteligentes
            .Include(c => c.Dispositivo)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<CerradurasInteligente?> GetById(int id)
    {
        return await _context.CerradurasInteligentes
            .Include(c => c.Dispositivo)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CerraduraId == id);
    }

    public async Task<IEnumerable<CerradurasInteligente>> GetByDispositivoId(Guid dispositivoId)
    {
        return await _context.CerradurasInteligentes
            .Where(c => c.DispositivoId == dispositivoId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<CerradurasInteligente>> GetByHabitacionId(int habitacionId)
    {
        return await _context.CerradurasInteligentes
            .Where(c => c.HabitacionId == habitacionId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<CerradurasInteligente>> GetByEstaActiva(bool estaActiva)
    {
        return await _context.CerradurasInteligentes
            .Where(c => c.EstaActiva == estaActiva)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(CerradurasInteligente cerradura, CancellationToken cancellationToken = default)
    {
        await _context.CerradurasInteligentes.AddAsync(cerradura, cancellationToken);
    }

    public async Task UpdateAsync(CerradurasInteligente cerradura, CancellationToken cancellationToken = default)
    {
        _context.CerradurasInteligentes.Update(cerradura);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(CerradurasInteligente cerradura, CancellationToken cancellationToken = default)
    {
        _context.CerradurasInteligentes.Remove(cerradura);
        await Task.CompletedTask;
    }
}
