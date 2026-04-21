using Notification.Domain.Data;

namespace Notification.Domain.Interfaces;

public interface IPreferenciasRepository
{
    Task<PreferenciaNotificacion?> GetByUsuarioIdAsync(string usuarioId, CancellationToken cancellationToken = default);
    Task<PreferenciaNotificacion> AddAsync(PreferenciaNotificacion preferencia, CancellationToken cancellationToken = default);
    Task UpdateAsync(PreferenciaNotificacion preferencia, CancellationToken cancellationToken = default);
}
