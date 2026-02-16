using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.RegistrosAcceso.Commands;

public class DeleteRegistrosAccesoCommandHandler : IRequestHandler<DeleteRegistrosAccesoCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRegistrosAccesoCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(DeleteRegistrosAccesoCommand request, CancellationToken cancellationToken)
    {
        var existeRegistro = await _unitOfWork.RegistrosAcceso.GetByIdAsync(request.RegistroId);
        
        if (existeRegistro == null)
        {
            return Result<int>.Failure("Registro de acceso no encontrado.");
        }

        await _unitOfWork.RegistrosAcceso.DeleteAsync(request.RegistroId);
        await _unitOfWork.SaveChangesAsync();
        
        return Result<int>.Success(request.RegistroId);
    }
}
