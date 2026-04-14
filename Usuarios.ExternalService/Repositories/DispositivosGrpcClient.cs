using Grpc.Contracts.Dispositivos;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Usuarios.Application.Interfaces;

namespace Usuarios.ExternalService.Repositories;

public class DispositivosGrpcClient : IDispositivosApiService
{
    private readonly PersonalEstado.PersonalEstadoClient _client;
    private readonly ILogger<DispositivosGrpcClient> _logger;

    public DispositivosGrpcClient(GrpcChannel channel, ILogger<DispositivosGrpcClient> logger)
    {
        _client = new PersonalEstado.PersonalEstadoClient(channel);
        _logger = logger;
    }

    public async Task SincronizarEstadoPersonalAsync(int personalId, bool estaActivo, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("gRPC: Sincronizando estado personal {PersonalId}, EstaActivo={EstaActivo}.", personalId, estaActivo);

        try
        {
            await _client.SincronizarEstadoPersonalAsync(
                new SincronizarEstadoPersonalRequest { PersonalId = personalId, EstaActivo = estaActivo },
                cancellationToken: cancellationToken);

            _logger.LogInformation("gRPC: Sincronización completada para Personal {PersonalId}.", personalId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC: Error al sincronizar estado del Personal {PersonalId}.", personalId);
        }
    }
}
