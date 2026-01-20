using MediatR;
using Usuarios.Application.Common;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Personal.Commands.DeletePersonal;

public class DeletePersonalCommandHandler : IRequestHandler<DeletePersonalCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePersonalCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeletePersonalCommand request, CancellationToken cancellationToken)
    {
        var personal = await _unitOfWork.Personal.GetByIdAsync(request.PersonalId);
        if (personal == null)
        {
            return Result<bool>.Failure("Personal no encontrado");
        }

        var subordinados = await _unitOfWork.Personal.GetBySupervisorAsync(request.PersonalId);
        if (subordinados.Any())
        {
            return Result<bool>.Failure("No se puede eliminar el personal porque tiene subordinados asignados");
        }

        await _unitOfWork.Personal.DeleteAsync(personal);
        await _unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true, "Personal eliminado exitosamente");
    }
}
