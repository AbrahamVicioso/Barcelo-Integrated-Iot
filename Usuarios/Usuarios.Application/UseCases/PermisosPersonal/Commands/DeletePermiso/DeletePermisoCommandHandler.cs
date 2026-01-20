using MediatR;
using Usuarios.Application.Common;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.PermisosPersonal.Commands.DeletePermiso;

public class DeletePermisoCommandHandler : IRequestHandler<DeletePermisoCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePermisoCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeletePermisoCommand request, CancellationToken cancellationToken)
    {
        var permiso = await _unitOfWork.PermisosPersonal.GetByIdAsync(request.PermisoId);
        if (permiso == null)
        {
            return Result<bool>.Failure("Permiso no encontrado");
        }

        await _unitOfWork.PermisosPersonal.DeleteAsync(permiso);
        await _unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true, "Permiso eliminado exitosamente");
    }
}
