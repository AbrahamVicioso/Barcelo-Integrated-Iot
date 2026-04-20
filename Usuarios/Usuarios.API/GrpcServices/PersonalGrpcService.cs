using Grpc.Contracts.Usuarios;
using Grpc.Core;
using MediatR;
using Usuarios.Application.UseCases.Personal.Queries.GetPersonalById;
using Usuarios.Application.UseCases.Personal.Queries.GetPersonalByUserId;

namespace Usuarios.API.GrpcServices;

public class PersonalGrpcService : Personal.PersonalBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PersonalGrpcService> _logger;

    public PersonalGrpcService(IMediator mediator, ILogger<PersonalGrpcService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<PersonalResponse> GetPersonalById(
        GetPersonalByIdRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC: GetPersonalById para id: {PersonalId}", request.PersonalId);

        try
        {
            var personal = await _mediator.Send(new GetPersonalByIdQuery(request.PersonalId));
            return MapToResponse(personal);
        }
        catch (Exception ex) when (IsNotFoundException(ex))
        {
            _logger.LogWarning("gRPC: Personal no encontrado para id: {PersonalId}", request.PersonalId);
            return new PersonalResponse { Found = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC: Error al obtener personal por id: {PersonalId}", request.PersonalId);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<PersonalResponse> GetPersonalByUserId(
        GetPersonalByUserIdRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC: GetPersonalByUserId para usuarioId: {UsuarioId}", request.UsuarioId);

        try
        {
            var personal = await _mediator.Send(new GetPersonalByUserIdQuery(request.UsuarioId));
            return MapToResponse(personal);
        }
        catch (Exception ex) when (IsNotFoundException(ex))
        {
            _logger.LogWarning("gRPC: Personal no encontrado para usuarioId: {UsuarioId}", request.UsuarioId);
            return new PersonalResponse { Found = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC: Error al obtener personal para usuarioId: {UsuarioId}", request.UsuarioId);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    private static bool IsNotFoundException(Exception ex) =>
        ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("no encontrado", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("no existe", StringComparison.OrdinalIgnoreCase);

    private static PersonalResponse MapToResponse(Usuarios.Application.DTOs.Personal.PersonalDto dto) => new()
    {
        Found = true,
        PersonalId = dto.PersonalId,
        UsuarioId = dto.UsuarioId ?? string.Empty,
        NombreCompleto = dto.NombreCompleto ?? string.Empty,
        Cargo = dto.NombrePuesto ?? string.Empty,
        Departamento = dto.NombreDepartamento ?? string.Empty,
        EstaActivo = dto.EstaActivo,
        CorreoElectronico = dto.CorreoElectronico ?? string.Empty,
    };
}