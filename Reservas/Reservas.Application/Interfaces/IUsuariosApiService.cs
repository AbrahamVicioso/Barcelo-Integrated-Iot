namespace Reservas.Application.Interfaces;

using Reservas.Application.DTOs;

public interface IUsuariosApiService
{
    Task<HuespedeDto?> GetHuespedByUsuarioIdAsync(string usuarioId, CancellationToken cancellationToken = default);
    Task<HuespedeDto?> GetHuespedByIdAsync(int huespedId, CancellationToken cancellationToken = default);
}
