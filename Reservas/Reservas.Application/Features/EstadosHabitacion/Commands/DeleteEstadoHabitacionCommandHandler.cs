using MediatR;
using Reservas.Application.Common;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.EstadosHabitacion.Commands;

public class DeleteEstadoHabitacionCommandHandler : IRequestHandler<DeleteEstadoHabitacionCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEstadoHabitacionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(DeleteEstadoHabitacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var estado = await _unitOfWork.EstadosHabitacion.GetById(request.EstadoHabitacionId);
            if (estado == null)
                return Result<int>.Failure($"Estado con ID {request.EstadoHabitacionId} no encontrado.");

            await _unitOfWork.EstadosHabitacion.DeleteAsync(estado, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(request.EstadoHabitacionId);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error al eliminar el estado: {ex.Message}");
        }
    }
}
