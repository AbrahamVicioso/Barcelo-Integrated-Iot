namespace Usuarios.Application.Interfaces;

/// <summary>
/// Publishes a notification so Dispositivos can sync ThingsBoard credentials
/// for the given habitacion after a personal permission is granted.
/// </summary>
public interface IPermisoHabitacionSyncProducer
{
    Task PublishAsync(int habitacionId, CancellationToken cancellationToken = default);
    Task PublishActividadAsync(int actividadId, CancellationToken cancellationToken = default);
}
