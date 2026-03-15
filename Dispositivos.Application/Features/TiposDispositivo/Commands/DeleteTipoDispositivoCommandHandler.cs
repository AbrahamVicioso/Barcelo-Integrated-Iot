using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.TiposDispositivo.Commands;

public class DeleteTipoDispositivoCommandHandler : IRequestHandler<DeleteTipoDispositivoCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTipoDispositivoCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(DeleteTipoDispositivoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var tipo = await _unitOfWork.TiposDispositivo.GetById(request.TipoDispositivoId);
            if (tipo == null)
                return Result<int>.Failure($"Tipo con ID {request.TipoDispositivoId} no encontrado.");

            await _unitOfWork.TiposDispositivo.DeleteAsync(tipo, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(request.TipoDispositivoId);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error al eliminar el tipo: {ex.Message}");
        }
    }
}
