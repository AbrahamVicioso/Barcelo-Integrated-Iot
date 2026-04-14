using AutoMapper;
using MediatR;
using Usuarios.Application.DTOs.PermisosPersonal;
using Usuarios.Application.Exceptions;
using Usuarios.Application.Interfaces;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.PermisosPersonal.Commands.UpdatePermiso;

public class UpdatePermisoCommandHandler : IRequestHandler<UpdatePermisoCommand, PermisosPersonalDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPermisoHabitacionSyncProducer _syncProducer;

    public UpdatePermisoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IPermisoHabitacionSyncProducer syncProducer)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _syncProducer = syncProducer;
    }

    public async Task<PermisosPersonalDto> Handle(UpdatePermisoCommand request, CancellationToken cancellationToken)
    {
        var permiso = await _unitOfWork.PermisosPersonal.GetByIdAsync(request.Permiso.PermisoId);
        if (permiso == null)
        {
            throw new NotFoundException("Permiso no encontrado");
        }

        permiso.FechaExpiracion = request.Permiso.FechaExpiracion;
        permiso.EstaActivo = request.Permiso.EstaActivo;
        permiso.Justificacion = request.Permiso.Justificacion;

        await _unitOfWork.PermisosPersonal.UpdateAsync(permiso);
        await _unitOfWork.SaveChangesAsync();

        if (permiso.HabitacionId.HasValue)
            await _syncProducer.PublishAsync(permiso.HabitacionId.Value, cancellationToken);

        var permisoDto = _mapper.Map<PermisosPersonalDto>(permiso);
        return permisoDto;
    }
}
