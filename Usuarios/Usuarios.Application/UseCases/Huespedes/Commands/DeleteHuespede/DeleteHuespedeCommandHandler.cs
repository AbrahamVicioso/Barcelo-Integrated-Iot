using MediatR;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Huespedes.Commands.DeleteHuespede;

public class DeleteHuespedeCommandHandler : IRequestHandler<DeleteHuespedeCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteHuespedeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteHuespedeCommand request, CancellationToken cancellationToken)
    {
        var huespede = await _unitOfWork.Huespedes.GetByIdAsync(request.HuespedId);
        if (huespede == null)
        {
            throw new Exception("Huésped no encontrado");
        }

        await _unitOfWork.Huespedes.DeleteAsync(huespede);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
