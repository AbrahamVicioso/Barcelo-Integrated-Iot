using MediatR;
using Reservas.Application.Common;
using Reservas.Domain.Interfaces;

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
            {
                return Result<bool>.Failure($"Reserva con ID {request.ReservaId} no encontrada.");
            }

            await _unitOfWork.Reservas.DeleteAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error al eliminar la reserva: {ex.Message}");
        }
    }
}
