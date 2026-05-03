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

    /// <summary>
    /// Finds all active/in-checkin reservations for the huesped, resolves their HabitacionIds,
    /// and calls SyncAsync for each. Never throws.
    /// </summary>
    Task SyncByHuespedIdAsync(int huespedId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds all habitaciones where the personal has an active permission,
    /// and calls SyncAsync for each. Never throws.
    /// </summary>
    Task SyncByPersonalIdAsync(int personalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs credentials for the activity lock identified by cerraduraId directly.
    /// Used after creating a credential for a recreational activity reservation.
    /// Never throws — errors are logged and swallowed.
    /// </summary>
    Task SyncByCerraduraIdAsync(int cerraduraId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the CerraduraId from the given ReservaActividadId then calls SyncByCerraduraIdAsync.
    /// No-op if the reserva has no activity lock. Never throws.
    /// </summary>
    Task SyncByReservaActividadIdAsync(int reservaActividadId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the active lock for the given actividad and calls SyncByCerraduraIdAsync.
    /// No-op if the actividad has no active lock. Never throws.
    /// </summary>
    Task SyncByActividadIdAsync(int actividadId, CancellationToken cancellationToken = default);
}
