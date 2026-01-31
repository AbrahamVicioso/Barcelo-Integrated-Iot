using MediatR;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.PermisosPersonal.Commands.DeletePermiso;

public class DeletePermisoCommandHandler : IRequestHandler<DeletePermisoCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePermisoCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeletePermisoCommand request, CancellationToken cancellationToken)
    {
        var permiso = await _unitOfWork.PermisosPersonal.GetByIdAsync(request.PermisoId);
        if (permiso == null)
        {
            throw new Exception("Permiso no encontrado");
        }

        await _unitOfWork.PermisosPersonal.DeleteAsync(permiso);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
