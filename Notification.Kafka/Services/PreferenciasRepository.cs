using Microsoft.EntityFrameworkCore;
using Notification.Domain.Data;
using Notification.Domain.Interfaces;
using Notification.Kafka.Data;

namespace Notification.Kafka.Services;

public class PreferenciasRepository : IPreferenciasRepository
{
    private readonly IDbContextFactory<NotificacionDbContext> _contextFactory;

    public PreferenciasRepository(IDbContextFactory<NotificacionDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<PreferenciaNotificacion?> GetByUsuarioIdAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.PreferenciasNotificacion
            .FirstOrDefaultAsync(p => p.UsuarioId == usuarioId, cancellationToken);
    }

    public async Task<PreferenciaNotificacion> AddAsync(PreferenciaNotificacion preferencia, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.PreferenciasNotificacion.Add(preferencia);
        await context.SaveChangesAsync(cancellationToken);
        return preferencia;
    }

    public async Task UpdateAsync(PreferenciaNotificacion preferencia, CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        context.PreferenciasNotificacion.Update(preferencia);
        await context.SaveChangesAsync(cancellationToken);
    }
}