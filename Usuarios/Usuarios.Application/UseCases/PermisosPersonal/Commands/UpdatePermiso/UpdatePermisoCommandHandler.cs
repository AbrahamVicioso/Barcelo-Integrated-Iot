using AutoMapper;
using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.PermisosPersonal;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.PermisosPersonal.Commands.UpdatePermiso;

public class UpdatePermisoCommandHandler : IRequestHandler<UpdatePermisoCommand, Result<PermisosPersonalDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdatePermisoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PermisosPersonalDto>> Handle(UpdatePermisoCommand request, CancellationToken cancellationToken)
    {
        var permiso = await _unitOfWork.PermisosPersonal.GetByIdAsync(request.Permiso.PermisoId);
        if (permiso == null)
        {
            return Result<PermisosPersonalDto>.Failure("Permiso no encontrado");
        }

        permiso.FechaExpiracion = request.Permiso.FechaExpiracion;
        permiso.EstaActivo = request.Permiso.EstaActivo;
        permiso.Justificacion = request.Permiso.Justificacion;

        await _unitOfWork.PermisosPersonal.UpdateAsync(permiso);
        await _unitOfWork.SaveChangesAsync();

        var permisoDto = _mapper.Map<PermisosPersonalDto>(permiso);
        return Result<PermisosPersonalDto>.Success(permisoDto, "Permiso actualizado exitosamente");
    }
}
