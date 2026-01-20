using MediatR;
using Reservas.Application.Common;
using Reservas.Domain.Interfaces;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public class DeleteReservaActividadCommandHandler : IRequestHandler<DeleteReservaActividadCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteReservaActividadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteReservaActividadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reserva = await _unitOfWork.ReservasActividades.GetByIdAsync(request.ReservaActividadId, cancellationToken);

            if (reserva == null)
            {
                return Result<bool>.Failure($"Reserva de actividad con ID {request.ReservaActividadId} no encontrada.");
            }

            await _unitOfWork.ReservasActividades.DeleteAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error al eliminar la reserva de actividad: {ex.Message}");
        }
    }
}
