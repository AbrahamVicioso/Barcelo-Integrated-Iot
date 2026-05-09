using MediatR;
using Usuarios.Application.Exceptions;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Departamento.Commands.DeleteDepartamento;

public class DeleteDepartamentoCommandHandler : IRequestHandler<DeleteDepartamentoCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDepartamentoCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteDepartamentoCommand request, CancellationToken cancellationToken)
    {
        var departamento = await _unitOfWork.Departamentos.GetByIdAsync(request.DepartamentoId);
        if (departamento == null)
            throw new NotFoundException($"Departamento con ID {request.DepartamentoId} no encontrado");

        var tienePersonal = await _unitOfWork.Personal.ExistsAsync(p => p.DepartamentoId == request.DepartamentoId);
        if (tienePersonal)
            throw new BusinessException("No se puede eliminar el departamento porque tiene personal asignado");

        departamento.EstaActivo = false;
        departamento.EliminadoEn = DateTime.Now;

        await _unitOfWork.Departamentos.UpdateAsync(departamento);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
