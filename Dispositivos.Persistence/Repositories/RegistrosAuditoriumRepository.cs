using Dispositivos.Domain.Entities;
using Dispositivos.Application.Interfaces;
using Dispositivos.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Dispositivos.Persistence.Repositories;

public class RegistrosAuditoriumRepository : IRegistrosAuditoriumRepository
{
    private readonly BarceloIoTDatabaseContext _context;

    public RegistrosAuditoriumRepository(BarceloIoTDatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RegistrosAuditorium>> GetAllAsync()
    {
        return await _context.RegistrosAuditoria
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<RegistrosAuditorium?> GetByIdAsync(int id)
    {
        return await _context.RegistrosAuditoria
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.AuditoriaId == id);
    }

    public async Task<IEnumerable<RegistrosAuditorium>> GetByUsuarioIdAsync(string usuarioId)
    {
        return await _context.RegistrosAuditoria
            .Where(r => r.UsuarioId == usuarioId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<RegistrosAuditorium>> GetByTipoEntidadAsync(string tipoEntidad)
    {
        return await _context.RegistrosAuditoria
            .Where(r => r.TipoEntidad == tipoEntidad)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<RegistrosAuditorium>> GetByHotelIdAsync(int hotelId)
    {
        return await _context.RegistrosAuditoria
            .Where(r => r.HotelId == hotelId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<RegistrosAuditorium> AddAsync(RegistrosAuditorium registro, CancellationToken cancellationToken = default)
    {
        await _context.RegistrosAuditoria.AddAsync(registro, cancellationToken);
        return registro;
    }

    public async Task UpdateAsync(RegistrosAuditorium registro, CancellationToken cancellationToken = default)
    {
        _context.RegistrosAuditoria.Update(registro);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var registro = await _context.RegistrosAuditoria.FindAsync(new object[] { id }, cancellationToken);
        if (registro != null)
        {
            _context.RegistrosAuditoria.Remove(registro);
        }
    }
}
