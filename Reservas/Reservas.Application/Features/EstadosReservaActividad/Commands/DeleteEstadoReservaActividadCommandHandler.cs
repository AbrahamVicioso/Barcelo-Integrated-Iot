using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.EstadosReservaActividad.Commands;

public class DeleteEstadoReservaActividadCommandHandler : IRequestHandler<DeleteEstadoReservaActividadCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEstadoReservaActividadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteEstadoReservaActividadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _unitOfWork.EstadosReservaActividad.GetByIdAsync(request.EstadoReservaActividadId, cancellationToken);
            if (entity == null)
                return Result<bool>.NotFound($"Estado con ID {request.EstadoReservaActividadId} no encontrado.");

            await _unitOfWork.EstadosReservaActividad.DeleteAsync(entity, cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error al eliminar el estado: {ex.Message}");
        }
    }
}