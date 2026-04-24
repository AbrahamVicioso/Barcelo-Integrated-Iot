using MediatR;
using Reservas.Application.Common;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entites;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public class DeleteReservaActividadMeCommandHandler : IRequestHandler<DeleteReservaActividadMeCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHuespedRepository _huespedRepository;

    public DeleteReservaActividadMeCommandHandler(
        IUnitOfWork unitOfWork,
        IHuespedRepository huespedRepository)
    {
        _unitOfWork = unitOfWork;
        _huespedRepository = huespedRepository;
    }

    public async Task<Result<bool>> Handle(DeleteReservaActividadMeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var huespedId = await _huespedRepository.GetHuespedIdByUserIdAsync(request.UsuarioId, cancellationToken);
            if (huespedId == null)
            {
                return Result<bool>.NotFound("Huésped no encontrado para este usuario.");
            }

            var reserva = await _unitOfWork.ReservasActividades.GetByIdAsync(request.ReservaActividadId, cancellationToken);
            if (reserva == null)
            {
                return Result<bool>.NotFound($"Reserva de actividad con ID {request.ReservaActividadId} no encontrada.");
            }

            if (reserva.HuespedId != huespedId.Value)
            {
                return Result<bool>.Failure("No tienes permiso para cancelar esta reserva.");
            }

            reserva.EstadoReservaActividadId = EstadoReservaActividad.Cancelada;
            reserva.Estado = "Cancelada";

            await _unitOfWork.ReservasActividades.UpdateAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error al eliminar la reserva: {ex.Message}");
        }
    }
}