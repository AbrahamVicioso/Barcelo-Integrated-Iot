using Microsoft.EntityFrameworkCore;
using Notification.Domain.Data;
using Notification.Domain.Interfaces;
using Notification.Kafka.Data;

namespace Notification.Kafka.Services;

public class NotificacionesRepository : INotificacionesRepository
{
    private readonly IDbContextFactory<NotificacionDbContext> _contextFactory;

    public NotificacionesRepository(IDbContextFactory<NotificacionDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<NotificacionEntity> AddAsync(NotificacionEntity notificacion, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Notificaciones.Add(notificacion);
        await context.SaveChangesAsync(cancellationToken);
        return notificacion;
    }

    public async Task<NotificacionEntity?> GetByIdAsync(int notificacionId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Notificaciones
            .FirstOrDefaultAsync(n => n.NotificacionId == notificacionId, cancellationToken);
    }

    public async Task<List<NotificacionEntity>> GetByUsuarioIdAsync(string usuarioId, int skip, int take, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.FechaEnvio)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByUsuarioIdAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Notificaciones
            .CountAsync(n => n.UsuarioId == usuarioId, cancellationToken);
    }

    public async Task UpdateAsync(NotificacionEntity notificacion, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.Notificaciones.Update(notificacion);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int notificacionId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var notificacion = await context.Notificaciones
            .FirstOrDefaultAsync(n => n.NotificacionId == notificacionId, cancellationToken);
        if (notificacion != null)
        {
            context.Notificaciones.Remove(notificacion);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}