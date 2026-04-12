using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;
using Reservas.Application.Common;
using Reservas.Application.Interfaces;
using System.Security.Claims;

namespace Reservas.Application.Features.Habitaciones.Commands;

public class UnlockDoorPersonalCommandHandler : IRequestHandler<UnlockDoorPersonalCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReservaKafkaProducer _kafkaProducer;
    private readonly ICredencialesAccesoService _credencialesService;
    private readonly IUsuariosApiService _usuariosApiService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UnlockDoorPersonalCommandHandler> _logger;

    public UnlockDoorPersonalCommandHandler(
        IUnitOfWork unitOfWork,
        IReservaKafkaProducer kafkaProducer,
        ICredencialesAccesoService credencialesService,
        IUsuariosApiService usuariosApiService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UnlockDoorPersonalCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _kafkaProducer = kafkaProducer;
        _credencialesService = credencialesService;
        _usuariosApiService = usuariosApiService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(UnlockDoorPersonalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var usuarioId = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? httpContext?.User.FindFirst("nameid")?.Value;

            if (string.IsNullOrEmpty(usuarioId))
                return Result<string>.Failure("No se pudo identificar al usuario autenticado.");

            // Buscar registro de Personal asociado al usuario del JWT
            var personal = await _credencialesService.GetPersonalByUsuarioIdAsync(usuarioId, cancellationToken);
            if (personal is null)
                return Result<string>.Failure("El usuario autenticado no tiene un perfil de personal activo en el sistema.");

            var (personalId, nombrePersonal) = personal.Value;

            var habitacion = await _unitOfWork.Habitaciones.GetById(request.HabitacionId);
            if (habitacion == null)
                return Result<string>.NotFound($"Habitación con ID {request.HabitacionId} no encontrada.");

            // Verificar que el personal tiene permiso activo para esta habitación
            var tienePermiso = await _credencialesService.PersonalTienePermisoAsync(
                personalId, request.HabitacionId, cancellationToken);

            if (!tienePermiso)
                return Result<string>.Failure($"El personal {nombrePersonal} no tiene permiso activo para la habitación {habitacion.NumeroHabitacion}.");

            var tieneCerradura = await _credencialesService.HabitacionTieneCerraduraActivaAsync(
                request.HabitacionId, cancellationToken);

            if (!tieneCerradura)
                return Result<string>.Failure("La habitación no tiene una cerradura inteligente activa asociada.");

            var direccionIp = httpContext?.Connection.RemoteIpAddress?.ToString();
            var infoDispositivo = httpContext?.Request.Headers["User-Agent"].ToString();

            // Obtener huéspedes de la reserva activa para notificarles
            var huespedInfos = new List<HuespedCheckInInfo>();
            var reservaId = await _credencialesService.GetReservaActivaByHabitacionIdAsync(
                request.HabitacionId, cancellationToken);

            if (reservaId.HasValue)
            {
                var reserva = await _unitOfWork.Reservas.GetByIdAsync(reservaId.Value, cancellationToken);
                if (reserva != null)
                {
                    var huespedIds = reserva.ReservaHuespedes
                        .Select(rh => rh.HuespedId)
                        .Append(reserva.HuespedId)
                        .Distinct()
                        .ToList();

                    foreach (var huespedId in huespedIds)
                    {
                        try
                        {
                            var h = await _usuariosApiService.GetHuespedByIdAsync(huespedId, cancellationToken);
                            huespedInfos.Add(new HuespedCheckInInfo
                            {
                                HuespedId = huespedId,
                                Email = h?.CorreoElectronico ?? string.Empty,
                                NombreCompleto = h?.NombreCompleto ?? string.Empty
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "No se pudo obtener email del huésped {HuespedId}", huespedId);
                            huespedInfos.Add(new HuespedCheckInInfo { HuespedId = huespedId });
                        }
                    }
                }
            }

            var personalUnlockEvent = new PersonalUnlockDoorEvent
            {
                HabitacionId = request.HabitacionId,
                NumeroHabitacion = habitacion.NumeroHabitacion,
                PersonalId = personalId,
                NombrePersonal = nombrePersonal,
                UsuarioId = usuarioId,
                DireccionIp = direccionIp,
                InfoDispositivo = infoDispositivo,
                Huespedes = huespedInfos
            };

            await _kafkaProducer.PublishPersonalUnlockDoorAsync(personalUnlockEvent, cancellationToken);

            _logger.LogInformation(
                "PersonalUnlockDoorEvent publicado para personal {PersonalId} ({NombrePersonal}), habitacion {HabitacionId}",
                personalId, nombrePersonal, request.HabitacionId);

            return Result<string>.Success($"Cerradura de habitación {habitacion.NumeroHabitacion} desbloqueada por {nombrePersonal}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desbloquear cerradura para habitacion {HabitacionId}", request.HabitacionId);
            return Result<string>.Failure($"Error al desbloquear la cerradura: {ex.Message}");
        }
    }
}
