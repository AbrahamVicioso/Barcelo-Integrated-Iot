namespace Reservas.Application.Interfaces;

public interface ICredencialesAccesoService
{
    /// <summary>Returns the CredencialId if the PIN is valid and active for the reserva, null otherwise.</summary>
    Task<int?> GetCredencialIdAsync(int reservaId, string pin, CancellationToken cancellationToken = default);
    Task<bool> HabitacionTieneCerraduraActivaAsync(int habitacionId, CancellationToken cancellationToken = default);
    /// <summary>Increments NumeroUsos and sets UltimaUso to now for the given credential.</summary>
    Task RegistrarUsoAsync(int credencialId, CancellationToken cancellationToken = default);
    /// <summary>Returns true if the personal has an active, non-expired permission for the given habitacion.</summary>
    Task<bool> PersonalTienePermisoAsync(int personalId, int habitacionId, CancellationToken cancellationToken = default);
    /// <summary>Returns the ReservaId of the active reservation for the given habitacion, or null if none.</summary>
    Task<int?> GetReservaActivaByHabitacionIdAsync(int habitacionId, CancellationToken cancellationToken = default);
    /// <summary>Returns the NombreCompleto of the personal, or null if not found.</summary>
    Task<string?> GetPersonalNombreAsync(int personalId, CancellationToken cancellationToken = default);
    /// <summary>Returns (PersonalId, NombreCompleto) for the given UsuarioId, or null if no personal record exists.</summary>
    Task<(int PersonalId, string NombreCompleto)?> GetPersonalByUsuarioIdAsync(string usuarioId, CancellationToken cancellationToken = default);
}
