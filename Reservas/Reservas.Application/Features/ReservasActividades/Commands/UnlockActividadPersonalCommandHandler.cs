using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;
using Reservas.Application.Common;
using Reservas.Application.Interfaces;
using System.Security.Claims;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public class UnlockActividadPersonalCommandHandler : IRequestHandler<UnlockActividadPersonalCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReservaActividadKafkaProducer _kafkaProducer;
    private readonly ICredencialesAccesoService _credencialesService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UnlockActividadPersonalCommandHandler> _logger;

    public UnlockActividadPersonalCommandHandler(
        IUnitOfWork unitOfWork,
        IReservaActividadKafkaProducer kafkaProducer,
        ICredencialesAccesoService credencialesService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UnlockActividadPersonalCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _kafkaProducer = kafkaProducer;
        _credencialesService = credencialesService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(UnlockActividadPersonalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var usuarioId = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? httpContext?.User.FindFirst("nameid")?.Value;

            if (string.IsNullOrEmpty(usuarioId))
                return Result<string>.Failure("No se pudo identificar al usuario autenticado.");

            var personal = await _credencialesService.GetPersonalByUsuarioIdAsync(usuarioId, cancellationToken);
            if (personal is null)
                return Result<string>.Failure("El usuario no tiene un perfil de personal activo.");

            var (personalId, nombrePersonal) = personal.Value;

            var actividad = await _unitOfWork.ActividadesRecreativas.GetByIdAsync(request.ActividadId, cancellationToken);
            if (actividad == null)
                return Result<string>.NotFound($"Actividad con ID {request.ActividadId} no encontrada.");

            var tieneCerradura = await _credencialesService.ActividadTieneCerraduraActivaAsync(
                request.ActividadId, cancellationToken);
            if (!tieneCerradura)
                return Result<string>.Failure("La actividad no tiene una cerradura inteligente activa.");

            var tienePermiso = await _credencialesService.PersonalTienePermisoActividadAsync(
                personalId, request.ActividadId, cancellationToken);
            if (!tienePermiso)
                return Result<string>.Failure($"El personal {nombrePersonal} no tiene permiso activo para esta actividad.");

            var direccionIp = httpContext?.Connection.RemoteIpAddress?.ToString();
            var infoDispositivo = httpContext?.Request.Headers["User-Agent"].ToString();

            await _credencialesService.RegistrarAccesoActividadAsync(request.ActividadId, null, cancellationToken);

            var personalUnlockEvent = new PersonalActividadUnlockDoorEvent
            {
                ActividadId = request.ActividadId,
                NombreActividad = actividad.NombreActividad ?? string.Empty,
                PersonalId = personalId,
                NombrePersonal = nombrePersonal,
                UsuarioId = usuarioId,
                DireccionIp = direccionIp,
                InfoDispositivo = infoDispositivo
            };

            await _kafkaProducer.PublishPersonalActividadUnlockDoorAsync(personalUnlockEvent, cancellationToken);

            _logger.LogInformation(
                "[UnlockActividadPersonal] Publicado para personal {PersonalId}, actividad {ActividadId}",
                personalId, request.ActividadId);

            return Result<string>.Success($"Cerradura de actividad '{actividad.NombreActividad}' desbloqueada por {nombrePersonal}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UnlockActividadPersonal] Error al desbloquear cerradura para actividad {ActividadId}", request.ActividadId);
            return Result<string>.Failure($"Error al desbloquear la cerradura: {ex.Message}");
        }
    }
}
