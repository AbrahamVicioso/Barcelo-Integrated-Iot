namespace Reservas.Application.Interfaces;

public interface ICredencialesAccesoService
{
    Task<bool> ValidatePinForReservaAsync(int reservaId, string pin, CancellationToken cancellationToken = default);
}
