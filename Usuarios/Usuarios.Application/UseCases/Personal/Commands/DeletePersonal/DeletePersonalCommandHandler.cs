using MediatR;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Personal.Commands.DeletePersonal;

public class DeletePersonalCommandHandler : IRequestHandler<DeletePersonalCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePersonalCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeletePersonalCommand request, CancellationToken cancellationToken)
    {
        var personal = await _unitOfWork.Personal.GetByIdAsync(request.PersonalId);
        if (personal == null)
        {
            throw new Exception("Personal no encontrado");
        }

        var subordinados = await _unitOfWork.Personal.GetBySupervisorAsync(request.PersonalId);
        if (subordinados.Any())
        {
            throw new Exception("No se puede eliminar el personal porque tiene subordinados asignados");
        }

        await _unitOfWork.Personal.DeleteAsync(personal);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
