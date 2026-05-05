using Dispositivos.Application.Interfaces;
using Grpc.Contracts.Usuarios;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace Dispositivos.Infrastructure.GrpcClients;

public class UsuariosGrpcClient : IUsuariosGrpcService
{
    private readonly Huesped.HuespedClient _huespedClient;
    private readonly Personal.PersonalClient _personalClient;
    private readonly ILogger<UsuariosGrpcClient> _logger;

    public UsuariosGrpcClient(GrpcChannel channel, ILogger<UsuariosGrpcClient> logger)
    {
        _huespedClient = new Huesped.HuespedClient(channel);
        _personalClient = new Personal.PersonalClient(channel);
        _logger = logger;
    }

    public async Task<int?> GetPersonalIdByUsuarioIdAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _personalClient.GetPersonalByUserIdAsync(
                new GetPersonalByUserIdRequest { UsuarioId = usuarioId },
                cancellationToken: cancellationToken);

            return response.Found ? response.PersonalId : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC: Error al obtener PersonalId para usuarioId: {UsuarioId}", usuarioId);
            throw new Exception($"Error al obtener personal desde Usuarios.API: {ex.Message}", ex);
        }
    }

    public async Task<int?> GetHuespedIdByUsuarioIdAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _huespedClient.GetHuespedByUserIdAsync(
                new GetHuespedByUserIdRequest { UsuarioId = usuarioId },
                cancellationToken: cancellationToken);

            return response.Found ? response.HuespedId : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC: Error al obtener HuespedId para usuarioId: {UsuarioId}", usuarioId);
            throw new Exception($"Error al obtener huésped desde Usuarios.API: {ex.Message}", ex);
        }
    }
}
