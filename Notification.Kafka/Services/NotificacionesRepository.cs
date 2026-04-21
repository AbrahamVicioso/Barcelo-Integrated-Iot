using Microsoft.EntityFrameworkCore;
using Notification.Domain.Data;
using Notification.Domain.Interfaces;
using Notification.Kafka.Data;

namespace Notification.Kafka.Services;

public class NotificacionesRepository : INotificacionesRepository
{
    private readonly NotificacionDbContext _context;

    public NotificacionesRepository(NotificacionDbContext context)
    {
        _context = context;
    }

    public async Task<NotificacionEntity> AddAsync(NotificacionEntity notificacion, CancellationToken cancellationToken = default)
    {
        _context.Notificaciones.Add(notificacion);
        await _context.SaveChangesAsync(cancellationToken);
        return notificacion;
    }

    public async Task<NotificacionEntity?> GetByIdAsync(int notificacionId, CancellationToken cancellationToken = default)
    {
        return await _context.Notificaciones
            .FirstOrDefaultAsync(n => n.NotificacionId == notificacionId, cancellationToken);
    }

    public async Task<List<NotificacionEntity>> GetByUsuarioIdAsync(string usuarioId, int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.FechaEnvio)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByUsuarioIdAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Notificaciones
            .CountAsync(n => n.UsuarioId == usuarioId, cancellationToken);
    }

    public async Task UpdateAsync(NotificacionEntity notificacion, CancellationToken cancellationToken = default)
    {
        _context.Notificaciones.Update(notificacion);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int notificacionId, CancellationToken cancellationToken = default)
    {
        var notificacion = await GetByIdAsync(notificacionId, cancellationToken);
        if (notificacion != null)
        {
            _context.Notificaciones.Remove(notificacion);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
