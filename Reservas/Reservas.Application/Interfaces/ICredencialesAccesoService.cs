namespace Reservas.Application.Interfaces;

public interface ICredencialesAccesoService
{
    /// <summary>Returns the CredencialId if the PIN is valid and active for the reserva, null otherwise.</summary>
    Task<int?> GetCredencialIdAsync(int reservaId, string pin, CancellationToken cancellationToken = default);
    Task<bool> HabitacionTieneCerraduraActivaAsync(int habitacionId, CancellationToken cancellationToken = default);
}
