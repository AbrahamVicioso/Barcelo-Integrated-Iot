using MediatR;
using Usuarios.Application.Exceptions;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Puesto.Commands.DeletePuesto;

public class DeletePuestoCommandHandler : IRequestHandler<DeletePuestoCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePuestoCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeletePuestoCommand request, CancellationToken cancellationToken)
    {
        var puesto = await _unitOfWork.Puestos.GetByIdAsync(request.PuestoId);
        if (puesto == null)
            throw new NotFoundException($"Puesto con ID {request.PuestoId} no encontrado");

        var tienePersonal = await _unitOfWork.Personal.ExistsAsync(p => p.PuestoId == request.PuestoId);
        if (tienePersonal)
            throw new BusinessException("No se puede eliminar el puesto porque tiene personal asignado");

        puesto.EstaActivo = false;
        puesto.EliminadoEn = DateTime.UtcNow;

        await _unitOfWork.Puestos.UpdateAsync(puesto);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
