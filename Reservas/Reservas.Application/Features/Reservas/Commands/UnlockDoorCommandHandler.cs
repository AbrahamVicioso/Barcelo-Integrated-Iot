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
    private readonly ILogger<UnlockDoorCommandHandler> _logger;

    public UnlockDoorCommandHandler(
        IUnitOfWork unitOfWork,
        IReservaKafkaProducer kafkaProducer,
        ICredencialesAccesoService credencialesService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UnlockDoorCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _kafkaProducer = kafkaProducer;
        _credencialesService = credencialesService;
        _httpContextAccessor = httpContextAccessor;
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

            if (!reserva.HabitacionId.HasValue)
                return Result<string>.Failure("La reserva no tiene una habitación asignada.");

            var tieneCerradura = await _credencialesService.HabitacionTieneCerraduraActivaAsync(
                reserva.HabitacionId.Value, cancellationToken);

            if (!tieneCerradura)
                return Result<string>.Failure("La habitación no tiene una cerradura inteligente activa asociada.");

            var credencialId = await _credencialesService.GetCredencialIdAsync(
                request.ReservaId, request.Pin, cancellationToken);

            if (credencialId is null)
                return Result<string>.Failure("PIN inválido o credencial de acceso no activa para esta reserva.");

            var httpContext = _httpContextAccessor.HttpContext;
            var usuarioId = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var direccionIp = httpContext?.Connection.RemoteIpAddress?.ToString();
            var infoDispositivo = httpContext?.Request.Headers["User-Agent"].ToString();

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
                "UnlockDoorEvent publicado para reserva {NumeroReserva}, habitacion {HabitacionId}",
                reserva.NumeroReserva, reserva.HabitacionId);

            return Result<string>.Success($"Cerradura de habitacion {reserva.HabitacionId.Value} desbloqueada para reserva {reserva.NumeroReserva}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desbloquear cerradura para reserva {ReservaId}", request.ReservaId);
            return Result<string>.Failure($"Error al desbloquear la cerradura: {ex.Message}");
        }
    }
}
