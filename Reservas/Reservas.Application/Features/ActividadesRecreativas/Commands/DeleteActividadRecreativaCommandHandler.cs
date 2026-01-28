using MediatR;
using Reservas.Application.Common;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.ActividadesRecreativas.Commands;

public class DeleteActividadRecreativaCommandHandler : IRequestHandler<DeleteActividadRecreativaCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteActividadRecreativaCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteActividadRecreativaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var actividad = await _unitOfWork.ActividadesRecreativas.GetByIdAsync(request.ActividadId, cancellationToken);

            if (actividad == null)
            {
                return Result<bool>.Failure($"Actividad con ID {request.ActividadId} no encontrada.");
            }

            await _unitOfWork.ActividadesRecreativas.DeleteAsync(actividad, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error al eliminar la actividad: {ex.Message}");
        }
    }
}
