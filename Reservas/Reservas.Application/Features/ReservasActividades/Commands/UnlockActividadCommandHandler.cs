using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;
using Reservas.Application.Common;
using Reservas.Application.Interfaces;
using System.Security.Claims;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public class UnlockActividadCommandHandler : IRequestHandler<UnlockActividadCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReservaActividadKafkaProducer _kafkaProducer;
    private readonly ICredencialesAccesoService _credencialesService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUsuariosApiService _usuariosApiService;
    private readonly ILogger<UnlockActividadCommandHandler> _logger;

    public UnlockActividadCommandHandler(
        IUnitOfWork unitOfWork,
        IReservaActividadKafkaProducer kafkaProducer,
        ICredencialesAccesoService credencialesService,
        IHttpContextAccessor httpContextAccessor,
        IUsuariosApiService usuariosApiService,
        ILogger<UnlockActividadCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _kafkaProducer = kafkaProducer;
        _credencialesService = credencialesService;
        _httpContextAccessor = httpContextAccessor;
        _usuariosApiService = usuariosApiService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(UnlockActividadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var actividad = await _unitOfWork.ActividadesRecreativas.GetByIdAsync(request.ActividadId, cancellationToken);
            if (actividad == null)
                return Result<string>.NotFound($"Actividad con ID {request.ActividadId} no encontrada.");

            var tieneCerradura = await _credencialesService.ActividadTieneCerraduraActivaAsync(
                request.ActividadId, cancellationToken);
            if (!tieneCerradura)
                return Result<string>.Failure("La actividad no tiene una cerradura inteligente activa.");

            var httpContext = _httpContextAccessor.HttpContext;
            var usuarioId = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? httpContext?.User.FindFirst("nameid")?.Value;
            var direccionIp = httpContext?.Connection.RemoteIpAddress?.ToString();
            var infoDispositivo = httpContext?.Request.Headers["User-Agent"].ToString();

            if (string.IsNullOrEmpty(usuarioId))
                return Result<string>.Failure("No se pudo identificar al usuario. Se requiere autenticación.");

            var huesped = await _usuariosApiService.GetHuespedByUsuarioIdAsync(usuarioId, cancellationToken);
            if (huesped == null)
                return Result<string>.Failure("No se encontró un huésped asociado a este usuario.");

            // Buscar reserva confirmada del huésped para esta actividad
            var reservas = await _unitOfWork.ReservasActividades.FindAsync(
                r => r.HuespedId == huesped.HuespedId
                  && r.ActividadId == request.ActividadId
                  && r.EstadoReservaActividadId == 2,
                cancellationToken);

            var reservaActividad = reservas.FirstOrDefault();
            if (reservaActividad == null)
                return Result<string>.Failure("No tiene una reserva confirmada para esta actividad.");

            // Validar que sea el día de la reserva y dentro del horario exacto de la actividad
            var now = DateTime.UtcNow;
            if (reservaActividad.FechaReserva.Date != now.Date)
                return Result<string>.Failure("La actividad no está disponible hoy.");

            if (now.TimeOfDay < actividad.HoraApertura || now.TimeOfDay > actividad.HoraCierre)
                return Result<string>.Failure($"La actividad solo está disponible de {actividad.HoraApertura:hh\\:mm} a {actividad.HoraCierre:hh\\:mm}.");

            int? credencialId = null;

            if (!string.IsNullOrWhiteSpace(request.Pin))
            {
                credencialId = await _credencialesService.GetCredencialActividadIdAsync(
                    reservaActividad.ReservaActividadId, request.Pin, cancellationToken);

                if (credencialId is null)
                    return Result<string>.Failure("PIN inválido o credencial de acceso no activa.");

                await _credencialesService.RegistrarAccesoActividadAsync(
                    request.ActividadId, credencialId.Value, cancellationToken);
            }
            else
            {
                await _credencialesService.RegistrarAccesoActividadAsync(
                    request.ActividadId, null, cancellationToken);
            }

            var unlockEvent = new ActividadUnlockDoorEvent
            {
                ReservaActividadId = reservaActividad.ReservaActividadId,
                ActividadId = request.ActividadId,
                NombreActividad = actividad.NombreActividad ?? string.Empty,
                CredencialId = credencialId,
                UsuarioId = usuarioId,
                DireccionIp = direccionIp,
                InfoDispositivo = infoDispositivo
            };

            await _kafkaProducer.PublishActividadUnlockDoorAsync(unlockEvent, cancellationToken);

            _logger.LogInformation(
                "[UnlockActividad] Publicado para actividad {ActividadId}, huesped {HuespedId}",
                request.ActividadId, huesped.HuespedId);

            return Result<string>.Success($"Cerradura de actividad '{actividad.NombreActividad}' desbloqueada.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UnlockActividad] Error al desbloquear cerradura para actividad {ActividadId}", request.ActividadId);
            return Result<string>.Failure($"Error al desbloquear la cerradura: {ex.Message}");
        }
    }
}
