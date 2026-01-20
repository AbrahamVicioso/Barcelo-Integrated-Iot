using AutoMapper;
using MediatR;
using Usuarios.Application.Common;
using Usuarios.Application.DTOs.PermisosPersonal;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.PermisosPersonal.Commands.CreatePermiso;

public class CreatePermisoCommandHandler : IRequestHandler<CreatePermisoCommand, Result<PermisosPersonalDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreatePermisoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PermisosPersonalDto>> Handle(CreatePermisoCommand request, CancellationToken cancellationToken)
    {
        var personal = await _unitOfWork.Personal.GetByIdAsync(request.Permiso.PersonalId);
        if (personal == null)
        {
            return Result<PermisosPersonalDto>.Failure("El personal especificado no existe");
        }

        if (!personal.EstaActivo)
        {
            return Result<PermisosPersonalDto>.Failure("No se pueden otorgar permisos a personal inactivo");
        }

        if (request.Permiso.EsTemporal && !request.Permiso.FechaExpiracion.HasValue)
        {
            return Result<PermisosPersonalDto>.Failure("Los permisos temporales deben tener fecha de expiración");
        }

        var permiso = _mapper.Map<Domain.Entities.PermisosPersonal>(request.Permiso);
        permiso.FechaOtorgamiento = DateTime.UtcNow;
        permiso.EstaActivo = true;

        var createdPermiso = await _unitOfWork.PermisosPersonal.AddAsync(permiso);
        await _unitOfWork.SaveChangesAsync();

        var permisoDto = _mapper.Map<PermisosPersonalDto>(createdPermiso);
        return Result<PermisosPersonalDto>.Success(permisoDto, "Permiso creado exitosamente");
    }
}
