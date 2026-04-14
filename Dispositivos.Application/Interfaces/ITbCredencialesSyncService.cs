namespace Dispositivos.Application.Interfaces;

/// <summary>
/// Synchronizes all active credentials (huéspedes PINs + personal permisos)
/// valid in the next 7 days for a room's smart lock to ThingsBoard shared attributes.
/// </summary>
public interface ITbCredencialesSyncService
{
    /// <summary>
    /// Finds the active lock for the given habitacion, collects all credentials
    /// valid in the next 7 days, and pushes them to the ThingsBoard device.
    /// No-op if the habitacion has no active lock.
    /// Never throws — errors are logged and swallowed.
    /// </summary>
    Task SyncAsync(int habitacionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the HabitacionId from the given ReservaId then calls SyncAsync.
    /// No-op if the reserva has no HabitacionId or the habitacion has no active lock.
    /// Never throws — errors are logged and swallowed.
    /// </summary>
    Task SyncByReservaIdAsync(int reservaId, CancellationToken cancellationToken = default);
}
