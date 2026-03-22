using Grpc.Contracts.Usuarios;
using Grpc.Core;
using MediatR;
using Usuarios.Application.UseCases.Huespedes.Queries.GetHuespedeById;
using Usuarios.Application.UseCases.Huespedes.Queries.GetHuespedeByUserId;

namespace Usuarios.API.GrpcServices;

public class HuespedeGrpcService : Huesped.HuespedBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<HuespedeGrpcService> _logger;

    public HuespedeGrpcService(IMediator mediator, ILogger<HuespedeGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<HuespedeResponse> GetHuespedByUserId(
        GetHuespedByUserIdRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC: GetHuespedByUserId para usuarioId: {UsuarioId}", request.UsuarioId);

        try
        {
            var huesped = await _mediator.Send(new GetHuespedeByUserIdQuery(request.UsuarioId));
            return MapToResponse(huesped);
        }
        catch (Exception ex) when (IsNotFoundException(ex))
        {
            _logger.LogWarning("gRPC: Huésped no encontrado para usuarioId: {UsuarioId}", request.UsuarioId);
            return new HuespedeResponse { Found = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC: Error al obtener huésped para usuarioId: {UsuarioId}", request.UsuarioId);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<HuespedeResponse> GetHuespedById(
        GetHuespedByIdRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC: GetHuespedById para id: {HuespedId}", request.HuespedId);

        try
        {
            var huesped = await _mediator.Send(new GetHuespedeByIdQuery(request.HuespedId));
            return MapToResponse(huesped);
        }
        catch (Exception ex) when (IsNotFoundException(ex))
        {
            _logger.LogWarning("gRPC: Huésped no encontrado para id: {HuespedId}", request.HuespedId);
            return new HuespedeResponse { Found = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC: Error al obtener huésped por id: {HuespedId}", request.HuespedId);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    private static bool IsNotFoundException(Exception ex) =>
        ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("no existe", StringComparison.OrdinalIgnoreCase);

    private static HuespedeResponse MapToResponse(Usuarios.Application.DTOs.Huespedes.HuespedeDto dto) => new()
    {
        Found = true,
        HuespedId = dto.HuespedId,
        UsuarioId = dto.UsuarioId ?? string.Empty,
        NombreCompleto = dto.NombreCompleto ?? string.Empty,
        TipoDocumento = dto.TipoDocumento ?? string.Empty,
        NumeroDocumento = dto.NumeroDocumento ?? string.Empty,
        Nacionalidad = dto.Nacionalidad ?? string.Empty,
        FechaNacimiento = dto.FechaNacimiento.ToString("O"),
        ContactoEmergencia = dto.ContactoEmergencia ?? string.Empty,
        TelefonoEmergencia = dto.TelefonoEmergencia ?? string.Empty,
        EsVip = dto.EsVip,
        FechaRegistro = dto.FechaRegistro.ToString("O"),
        PreferenciasAlimentarias = dto.PreferenciasAlimentarias ?? string.Empty,
        NotasEspeciales = dto.NotasEspeciales ?? string.Empty,
        CorreoElectronico = dto.CorreoElectronico ?? string.Empty,
    };
}
