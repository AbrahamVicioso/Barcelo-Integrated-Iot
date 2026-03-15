using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.EstadosDispositivo.Commands;

public class DeleteEstadoDispositivoCommandHandler : IRequestHandler<DeleteEstadoDispositivoCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEstadoDispositivoCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(DeleteEstadoDispositivoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var estado = await _unitOfWork.EstadosDispositivo.GetById(request.EstadoDispositivoId);
            if (estado == null)
                return Result<int>.Failure($"Estado con ID {request.EstadoDispositivoId} no encontrado.");

            await _unitOfWork.EstadosDispositivo.DeleteAsync(estado, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(request.EstadoDispositivoId);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error al eliminar el estado: {ex.Message}");
        }
    }
}
