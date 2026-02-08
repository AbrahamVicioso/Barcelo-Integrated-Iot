using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.Dispositivos.Commands;

public class DeleteDispositivoCommandHandler : IRequestHandler<DeleteDispositivoCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDispositivoCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteDispositivoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var dispositivo = await _unitOfWork.Dispositivos.GetById(request.DispositivoId);

            if (dispositivo == null)
            {
                return Result<bool>.Failure($"Dispositivo con ID {request.DispositivoId} no encontrado.");
            }

            await _unitOfWork.Dispositivos.DeleteAsync(dispositivo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error al eliminar el dispositivo: {ex.Message}");
        }
    }
}
