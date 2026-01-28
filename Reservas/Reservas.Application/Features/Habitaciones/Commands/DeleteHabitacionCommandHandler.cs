using MediatR;
using Reservas.Application.Common;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Habitaciones.Commands;

public class DeleteHabitacionCommandHandler : IRequestHandler<DeleteHabitacionCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteHabitacionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteHabitacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var habitacion = await _unitOfWork.Habitaciones.GetById(request.HabitacionId);

            if (habitacion == null)
            {
                return Result<bool>.Failure($"Habitación con ID {request.HabitacionId} no encontrada.");
            }

            await _unitOfWork.Habitaciones.DeleteAsync(habitacion, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error al eliminar la habitación: {ex.Message}");
        }
    }
}
