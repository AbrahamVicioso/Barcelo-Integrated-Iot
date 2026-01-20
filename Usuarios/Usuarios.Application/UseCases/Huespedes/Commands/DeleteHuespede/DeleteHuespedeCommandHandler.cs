using MediatR;
using Usuarios.Application.Common;
using Usuarios.Domain.Interfaces;

namespace Usuarios.Application.UseCases.Huespedes.Commands.DeleteHuespede;

public class DeleteHuespedeCommandHandler : IRequestHandler<DeleteHuespedeCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteHuespedeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteHuespedeCommand request, CancellationToken cancellationToken)
    {
        var huespede = await _unitOfWork.Huespedes.GetByIdAsync(request.HuespedId);
        if (huespede == null)
        {
            return Result<bool>.Failure("Huésped no encontrado");
        }

        await _unitOfWork.Huespedes.DeleteAsync(huespede);
        await _unitOfWork.SaveChangesAsync();

        return Result<bool>.Success(true, "Huésped eliminado exitosamente");
    }
}
