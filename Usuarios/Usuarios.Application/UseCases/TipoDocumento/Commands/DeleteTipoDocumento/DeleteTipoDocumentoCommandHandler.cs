using MediatR;
using Usuarios.Application.Exceptions;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.TipoDocumento.Commands.DeleteTipoDocumento;

public class DeleteTipoDocumentoCommandHandler : IRequestHandler<DeleteTipoDocumentoCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTipoDocumentoCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteTipoDocumentoCommand request, CancellationToken cancellationToken)
    {
        var tipo = await _unitOfWork.TiposDocumento.GetByIdAsync(request.TipoDocumentoId);
        if (tipo == null)
            throw new NotFoundException($"Tipo de documento con ID {request.TipoDocumentoId} no encontrado");

        var tieneHuespedes = await _unitOfWork.Huespedes.ExistsAsync(h => h.TipoDocumentoId == request.TipoDocumentoId);
        if (tieneHuespedes)
            throw new BusinessException("No se puede eliminar el tipo de documento porque tiene huéspedes asociados");

        tipo.EstaActivo = false;
        tipo.EliminadoEn = DateTime.UtcNow;

        await _unitOfWork.TiposDocumento.UpdateAsync(tipo);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
