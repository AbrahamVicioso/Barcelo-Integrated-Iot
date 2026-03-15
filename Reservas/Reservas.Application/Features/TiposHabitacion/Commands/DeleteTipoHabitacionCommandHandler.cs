using MediatR;
using Reservas.Application.Common;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.TiposHabitacion.Commands;

public class DeleteTipoHabitacionCommandHandler : IRequestHandler<DeleteTipoHabitacionCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTipoHabitacionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteTipoHabitacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tipo = await _unitOfWork.TiposHabitacion.GetById(request.TipoHabitacionId);
            if (tipo == null)
                return Result<bool>.Failure($"Tipo de habitacion con ID {request.TipoHabitacionId} no encontrado.");

            await _unitOfWork.TiposHabitacion.DeleteAsync(tipo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error al eliminar el tipo de habitacion: {ex.Message}");
        }
    }
}
