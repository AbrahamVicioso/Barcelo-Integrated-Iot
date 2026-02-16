using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.Interfaces;

namespace Dispositivos.Application.Features.MantenimientoCerradura.Commands;

public class DeleteMantenimientoCerraduraCommandHandler : IRequestHandler<DeleteMantenimientoCerraduraCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMantenimientoCerraduraRepository _mantenimientoRepository;

    public DeleteMantenimientoCerraduraCommandHandler(
        IUnitOfWork unitOfWork,
        IMantenimientoCerraduraRepository mantenimientoRepository)
    {
        _unitOfWork = unitOfWork;
        _mantenimientoRepository = mantenimientoRepository;
    }

    public async Task<Result<bool>> Handle(DeleteMantenimientoCerraduraCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var mantenimiento = await _mantenimientoRepository.GetById(request.MantenimientoId);

            if (mantenimiento == null)
            {
                return Result<bool>.Failure($"Mantenimiento con ID {request.MantenimientoId} no encontrado.");
            }

            await _mantenimientoRepository.DeleteAsync(mantenimiento, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error al eliminar el mantenimiento de cerradura: {ex.Message}");
        }
    }
}
