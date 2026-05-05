namespace Dispositivos.Application.Interfaces;

public interface IUsuariosGrpcService
{
    Task<int?> GetPersonalIdByUsuarioIdAsync(string usuarioId, CancellationToken cancellationToken = default);
    Task<int?> GetHuespedIdByUsuarioIdAsync(string usuarioId, CancellationToken cancellationToken = default);
}
