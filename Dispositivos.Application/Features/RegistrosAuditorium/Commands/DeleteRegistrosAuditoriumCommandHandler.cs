using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.RegistrosAuditorium.Commands;

public class DeleteRegistrosAuditoriumCommandHandler : IRequestHandler<DeleteRegistrosAuditoriumCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRegistrosAuditoriumCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(DeleteRegistrosAuditoriumCommand request, CancellationToken cancellationToken)
    {
        var existeRegistro = await _unitOfWork.RegistrosAuditorium.GetByIdAsync(request.RegistroId);
        
        if (existeRegistro == null)
        {
            return Result<int>.Failure("Registro de auditoría no encontrado.");
        }

        await _unitOfWork.RegistrosAuditorium.DeleteAsync(request.RegistroId);
        await _unitOfWork.SaveChangesAsync();
        
        return Result<int>.Success(request.RegistroId);
    }
}
