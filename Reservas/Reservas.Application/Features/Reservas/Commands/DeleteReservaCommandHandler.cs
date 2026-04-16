using MediatR;
using Reservas.Application.Common;
using Reservas.Application.Interfaces;


namespace Reservas.Application.Features.Reservas.Commands;

public class DeleteReservaCommandHandler : IRequestHandler<DeleteReservaCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReservaKafkaProducer _kafkaProducer;

    public DeleteReservaCommandHandler(IUnitOfWork unitOfWork, IReservaKafkaProducer kafkaProducer)
    {
        _unitOfWork = unitOfWork;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<Result<bool>> Handle(DeleteReservaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reserva = await _unitOfWork.Reservas.GetByIdAsync(request.ReservaId, cancellationToken);

            if (reserva == null)
                return Result<bool>.Failure($"Reserva con ID {request.ReservaId} no encontrada.");

            if (reserva.EstadoReservaId == 4)
                return Result<bool>.Failure("La reserva ya está cancelada.");

            var habitacionId = reserva.HabitacionId;

            reserva.EstadoReservaId = 4;
            reserva.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.Reservas.UpdateAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Sync ThingsBoard para que las credenciales de esta reserva desaparezcan del dispositivo
            if (habitacionId.HasValue)
                await _kafkaProducer.PublishHabitacionSyncAsync(habitacionId.Value, cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error al cancelar la reserva: {ex.Message}");
        }
    }
}
