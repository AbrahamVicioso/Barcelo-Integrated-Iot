using Dispositivos.Domain.Entities;
using Dispositivos.Application.Interfaces;
using Dispositivos.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Dispositivos.Persistence.Repositories;

public class DispositivoRepository : IDispositivoRepository
{
    private readonly BarceloIoTDatabaseContext _context;

    public DispositivoRepository(BarceloIoTDatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Dispositivo>> GetAll()
    {
        return await _context.Dispositivos
            .Include(d => d.CerradurasInteligentes)
            .Include(d => d.MantenimientoCerraduras)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Dispositivo?> GetById(int id)
    {
        return await _context.Dispositivos
            .Include(d => d.CerradurasInteligentes)
            .Include(d => d.MantenimientoCerraduras)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DispositivoId == id);
    }

    public async Task<IEnumerable<Dispositivo>> GetByHotelId(int hotelId)
    {
        return await _context.Dispositivos
            .Where(d => d.HotelId == hotelId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Dispositivo>> GetByTipoDispositivo(string tipoDispositivo)
    {
        return await _context.Dispositivos
            .Where(d => d.TipoDispositivo == tipoDispositivo)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Dispositivo>> GetByEstaEnLinea(bool estaEnLinea)
    {
        return await _context.Dispositivos
            .Where(d => d.EstaEnLinea == estaEnLinea)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Dispositivo dispositivo, CancellationToken cancellationToken = default)
    {
        await _context.Dispositivos.AddAsync(dispositivo, cancellationToken);
    }

    public async Task UpdateAsync(Dispositivo dispositivo, CancellationToken cancellationToken = default)
    {
        _context.Dispositivos.Update(dispositivo);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Dispositivo dispositivo, CancellationToken cancellationToken = default)
    {
        _context.Dispositivos.Remove(dispositivo);
        await Task.CompletedTask;
    }
}
