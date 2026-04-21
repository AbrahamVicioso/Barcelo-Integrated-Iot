using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;
using Reservas.Application.Common;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entites;
using System.Security.Claims;

namespace Reservas.Application.Features.Reservas.Commands;

public class UnlockDoorCommandHandler : IRequestHandler<UnlockDoorCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReservaKafkaProducer _kafkaProducer;
    private readonly ICredencialesAccesoService _credencialesService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUsuariosApiService _usuariosApiService;
    private readonly ILogger<UnlockDoorCommandHandler> _logger;

    public UnlockDoorCommandHandler(
        IUnitOfWork unitOfWork,
        IReservaKafkaProducer kafkaProducer,
        ICredencialesAccesoService credencialesService,
        IHttpContextAccessor httpContextAccessor,
        IUsuariosApiService usuariosApiService,
        ILogger<UnlockDoorCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _kafkaProducer = kafkaProducer;
        _credencialesService = credencialesService;
        _httpContextAccessor = httpContextAccessor;
        _usuariosApiService = usuariosApiService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(UnlockDoorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reserva = await _unitOfWork.Reservas.GetByIdAsync(request.ReservaId, cancellationToken);

            if (reserva == null)
                return Result<string>.Failure($"Reserva con ID {request.ReservaId} no encontrada.");

            if (reserva.EstadoReservaId != EstadoReserva.Activa)
                return Result<string>.Failure("La reserva no tiene el estado de check-in realizado.");

            // Validar fechas de check-in y check-out
            var hoy = DateTime.UtcNow.Date;
            if (reserva.FechaCheckIn > hoy || reserva.FechaCheckOut < hoy)
                return Result<string>.Failure("La reserva no está dentro del rango de fechas válido para desbloquear la cerradura.");

            if (!reserva.HabitacionId.HasValue)
                return Result<string>.Failure("La reserva no tiene una habitación asignada.");

            var tieneCerradura = await _credencialesService.HabitacionTieneCerraduraActivaAsync(
                reserva.HabitacionId.Value, cancellationToken);

            if (!tieneCerradura)
                return Result<string>.Failure("La habitación no tiene una cerradura inteligente activa asociada.");

            var httpContext = _httpContextAccessor.HttpContext;
            var usuarioId = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var direccionIp = httpContext?.Connection.RemoteIpAddress?.ToString();
            var infoDispositivo = httpContext?.Request.Headers["User-Agent"].ToString();

            int? credencialId = null;

            // Flujo con PIN (método original)
            if (!string.IsNullOrWhiteSpace(request.Pin))
            {
                credencialId = await _credencialesService.GetCredencialIdAsync(
                    request.ReservaId, request.Pin, cancellationToken);

                if (credencialId is null)
                    return Result<string>.Failure("PIN inválido o credencial de acceso no activa para esta reserva.");

                await _credencialesService.RegistrarAccesoAsync(reserva.HabitacionId.Value, credencialId.Value, cancellationToken);

                _logger.LogInformation(
                    "[UnlockDoor] Acceso con PIN para reserva {NumeroReserva}, credencial {CredencialId}",
                    reserva.NumeroReserva, credencialId);
            }
            // Flujo sin PIN (con JWT del huésped)
            else
            {
                if (string.IsNullOrEmpty(usuarioId))
                    return Result<string>.Failure("No se pudo identificar al usuario. Se requiere autenticación.");

                _logger.LogInformation(
                    "[UnlockDoor] Intentando unlock sin PIN para reserva {ReservaId} con UsuarioId {UsuarioId}",
                    request.ReservaId, usuarioId);

                // Obtener HuespedId desde Usuarios.API
                var huesped = await _usuariosApiService.GetHuespedByUsuarioIdAsync(usuarioId, cancellationToken);

                if (huesped == null)
                    return Result<string>.Failure("No se encontró un huésped asociado a este usuario.");

                // Validar que el huésped tiene permisos para desbloquear
                bool esHuespedTitular = reserva.HuespedId == huesped.HuespedId;
                bool estaEnReservaHuespedes = reserva.ReservaHuespedes?.Any(rh =>
                    rh.HuespedId == huesped.HuespedId && rh.PuedeDesbloquearCerradura) ?? false;

                if (!esHuespedTitular && !estaEnReservaHuespedes)
                {
                    _logger.LogWarning(
                        "[UnlockDoor] Huésped {HuespedId} no tiene permisos para desbloquear reserva {ReservaId}. " +
                        "EsHuespedTitular: {EsHuespedTitular}, EstaEnReservaHuespedes: {EstaEnReservaHuespedes}",
                        huesped.HuespedId, request.ReservaId, esHuespedTitular, estaEnReservaHuespedes);

                    return Result<string>.Failure(
                        "No tiene permisos para desbloquear esta habitación. " +
                        "Debe ser el huésped titular o tener permisos de desbloqueo asignados.");
                }

                // Registrar acceso sin credencial
                await _credencialesService.RegistrarAccesoAsync(reserva.HabitacionId.Value, null, cancellationToken);

                _logger.LogInformation(
                    "[UnlockDoor] Acceso sin PIN autorizado para huésped {HuespedId}, reserva {NumeroReserva}. " +
                    "EsHuespedTitular: {EsHuespedTitular}, TienePermisoExplicito: {TienePermisoExplicito}",
                    huesped.HuespedId, reserva.NumeroReserva, esHuespedTitular, estaEnReservaHuespedes);
            }

            var unlockDoorEvent = new UnlockDoorEvent
            {
                ReservaId = reserva.ReservaId,
                HabitacionId = reserva.HabitacionId.Value,
                NumeroReserva = reserva.NumeroReserva,
                CredencialId = credencialId,
                UsuarioId = usuarioId,
                DireccionIp = direccionIp,
                InfoDispositivo = infoDispositivo
            };

            await _kafkaProducer.PublishUnlockDoorAsync(unlockDoorEvent, cancellationToken);

            _logger.LogInformation(
                "[UnlockDoor] UnlockDoorEvent publicado para reserva {NumeroReserva}, habitacion {HabitacionId}",
                reserva.NumeroReserva, reserva.HabitacionId);

            return Result<string>.Success($"Cerradura de habitacion {reserva.HabitacionId.Value} desbloqueada para reserva {reserva.NumeroReserva}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UnlockDoor] Error al desbloquear cerradura para reserva {ReservaId}", request.ReservaId);
            return Result<string>.Failure($"Error al desbloquear la cerradura: {ex.Message}");
        }
    }
}
