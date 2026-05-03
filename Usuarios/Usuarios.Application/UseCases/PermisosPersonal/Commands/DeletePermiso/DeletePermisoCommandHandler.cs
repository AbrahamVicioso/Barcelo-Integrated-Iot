using MediatR;
using Usuarios.Application.Exceptions;
using Usuarios.Application.Interfaces;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.PermisosPersonal.Commands.DeletePermiso;

public class DeletePermisoCommandHandler : IRequestHandler<DeletePermisoCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermisoHabitacionSyncProducer _syncProducer;

    public DeletePermisoCommandHandler(IUnitOfWork unitOfWork, IPermisoHabitacionSyncProducer syncProducer)
    {
        _unitOfWork = unitOfWork;
        _syncProducer = syncProducer;
    }

    public async Task<bool> Handle(DeletePermisoCommand request, CancellationToken cancellationToken)
    {
        var permiso = await _unitOfWork.PermisosPersonal.GetByIdAsync(request.PermisoId);
        if (permiso == null)
        {
            throw new NotFoundException("Permiso no encontrado");
        }

        var habitacionId = permiso.HabitacionId;
        var actividadId = permiso.ActividadId;

        await _unitOfWork.PermisosPersonal.DeleteAsync(permiso);
        await _unitOfWork.SaveChangesAsync();

        if (habitacionId.HasValue)
            await _syncProducer.PublishAsync(habitacionId.Value, cancellationToken);
        else if (actividadId.HasValue)
            await _syncProducer.PublishActividadAsync(actividadId.Value, cancellationToken);

        return true;
    }
}
