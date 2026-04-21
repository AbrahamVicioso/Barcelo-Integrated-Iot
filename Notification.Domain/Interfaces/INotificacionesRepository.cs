using Notification.Domain.Data;

namespace Notification.Domain.Interfaces;

public interface INotificacionesRepository
{
    Task<NotificacionEntity> AddAsync(NotificacionEntity notificacion, CancellationToken cancellationToken = default);
    Task<NotificacionEntity?> GetByIdAsync(int notificacionId, CancellationToken cancellationToken = default);
    Task<List<NotificacionEntity>> GetByUsuarioIdAsync(string usuarioId, int skip, int take, CancellationToken cancellationToken = default);
    Task<int> CountByUsuarioIdAsync(string usuarioId, CancellationToken cancellationToken = default);
    Task UpdateAsync(NotificacionEntity notificacion, CancellationToken cancellationToken = default);
    Task DeleteAsync(int notificacionId, CancellationToken cancellationToken = default);
}
