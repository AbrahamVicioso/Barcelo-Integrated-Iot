using MediatR;
using Reservas.Application.Common;
using Reservas.Application.Interfaces;


namespace Reservas.Application.Features.Reservas.Commands;

public class DeleteReservaCommandHandler : IRequestHandler<DeleteReservaCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteReservaCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

            reserva.EstadoReservaId = 4;
            reserva.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.Reservas.UpdateAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error al cancelar la reserva: {ex.Message}");
        }
    }
}
