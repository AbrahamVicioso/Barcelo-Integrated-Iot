using MediatR;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;
using Reservas.Application.Common;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Reservas.Commands;

public class UnlockDoorCommandHandler : IRequestHandler<UnlockDoorCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReservaKafkaProducer _kafkaProducer;
    private readonly ILogger<UnlockDoorCommandHandler> _logger;

    public UnlockDoorCommandHandler(
        IUnitOfWork unitOfWork,
        IReservaKafkaProducer kafkaProducer,
        ILogger<UnlockDoorCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(UnlockDoorCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reserva = await _unitOfWork.Reservas.GetByIdAsync(request.ReservaId, cancellationToken);

            if (reserva == null)
                return Result<string>.Failure($"Reserva con ID {request.ReservaId} no encontrada.");

            if (!reserva.HabitacionId.HasValue)
                return Result<string>.Failure("La reserva no tiene una habitación asignada.");

            var unlockDoorEvent = new UnlockDoorEvent
            {
                ReservaId = reserva.ReservaId,
                HabitacionId = reserva.HabitacionId.Value,
                NumeroReserva = reserva.NumeroReserva
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
