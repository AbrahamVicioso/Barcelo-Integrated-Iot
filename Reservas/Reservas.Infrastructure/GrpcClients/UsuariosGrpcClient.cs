using Grpc.Contracts.Usuarios;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Infrastructure.GrpcClients;

public class UsuariosGrpcClient : IUsuariosApiService
{
    private readonly Huesped.HuespedClient _client;
    private readonly ILogger<UsuariosGrpcClient> _logger;

    public UsuariosGrpcClient(GrpcChannel channel, ILogger<UsuariosGrpcClient> logger)
    {
        _client = new Huesped.HuespedClient(channel);
        _logger = logger;
    }

    public async Task<HuespedeDto?> GetHuespedByUsuarioIdAsync(string usuarioId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("gRPC: Obteniendo huésped para usuarioId: {UsuarioId}", usuarioId);

        try
        {
            var response = await _client.GetHuespedByUserIdAsync(
                new GetHuespedByUserIdRequest { UsuarioId = usuarioId },
                cancellationToken: cancellationToken);

            if (!response.Found)
            {
                _logger.LogWarning("gRPC: Huésped no encontrado para usuarioId: {UsuarioId}", usuarioId);
                return null;
            }

            _logger.LogInformation("gRPC: Huésped encontrado: {HuespedId}", response.HuespedId);
            return MapToDto(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC: Error al obtener huésped para usuarioId: {UsuarioId}", usuarioId);
            throw new Exception($"Error al obtener el huésped desde Usuarios.API (gRPC): {ex.Message}", ex);
        }
    }

    public async Task<HuespedeDto?> GetHuespedByIdAsync(int huespedId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("gRPC: Obteniendo huésped por ID: {HuespedId}", huespedId);

        try
        {
            var response = await _client.GetHuespedByIdAsync(
                new GetHuespedByIdRequest { HuespedId = huespedId },
                cancellationToken: cancellationToken);

            if (!response.Found)
            {
                _logger.LogWarning("gRPC: Huésped no encontrado con ID: {HuespedId}", huespedId);
                return null;
            }

            _logger.LogInformation("gRPC: Huésped encontrado: {HuespedId}", response.HuespedId);
            return MapToDto(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC: Error al obtener huésped por ID: {HuespedId}", huespedId);
            throw new Exception($"Error al obtener el huésped desde Usuarios.API (gRPC): {ex.Message}", ex);
        }
    }

    private static HuespedeDto MapToDto(HuespedeResponse r) => new()
    {
        HuespedId = r.HuespedId,
        UsuarioId = r.UsuarioId,
        NombreCompleto = r.NombreCompleto,
        TipoDocumento = r.TipoDocumento,
        NumeroDocumento = r.NumeroDocumento,
        Nacionalidad = r.Nacionalidad,
        FechaNacimiento = string.IsNullOrEmpty(r.FechaNacimiento)
            ? DateTime.MinValue
            : DateTime.Parse(r.FechaNacimiento),
        ContactoEmergencia = string.IsNullOrEmpty(r.ContactoEmergencia) ? null : r.ContactoEmergencia,
        TelefonoEmergencia = string.IsNullOrEmpty(r.TelefonoEmergencia) ? null : r.TelefonoEmergencia,
        EsVip = r.EsVip,
        FechaRegistro = string.IsNullOrEmpty(r.FechaRegistro)
            ? DateTime.MinValue
            : DateTime.Parse(r.FechaRegistro),
        PreferenciasAlimentarias = string.IsNullOrEmpty(r.PreferenciasAlimentarias) ? null : r.PreferenciasAlimentarias,
        NotasEspeciales = string.IsNullOrEmpty(r.NotasEspeciales) ? null : r.NotasEspeciales,
        CorreoElectronico = string.IsNullOrEmpty(r.CorreoElectronico) ? null : r.CorreoElectronico,
    };
}
