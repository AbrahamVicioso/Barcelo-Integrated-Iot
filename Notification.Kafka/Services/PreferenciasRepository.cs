using Microsoft.EntityFrameworkCore;
using Notification.Domain.Data;
using Notification.Domain.Interfaces;
using Notification.Kafka.Data;

namespace Notification.Kafka.Services;

public class PreferenciasRepository : IPreferenciasRepository
{
    private readonly NotificacionDbContext _context;

    public PreferenciasRepository(NotificacionDbContext context)
    {
        _context = context;
    }

    public async Task<PreferenciaNotificacion?> GetByUsuarioIdAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.PreferenciasNotificacion
            .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId, cancellationToken);
    }

    public async Task<PreferenciaNotificacion> AddAsync(PreferenciaNotificacion preferencia, CancellationToken cancellationToken = default)
    {
        _context.PreferenciasNotificacion.Add(preferencia);
        await _context.SaveChangesAsync(cancellationToken);
        return preferencia;
    }

    public async Task UpdateAsync(PreferenciaNotificacion preferencia, CancellationToken cancellationToken = default)
    {
        _context.PreferenciasNotificacion.Update(preferencia);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
