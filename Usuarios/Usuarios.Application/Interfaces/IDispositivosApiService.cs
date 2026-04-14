namespace Usuarios.Application.Interfaces;

public interface IDispositivosApiService
{
    Task SincronizarEstadoPersonalAsync(int personalId, bool estaActivo, CancellationToken cancellationToken = default);
}
