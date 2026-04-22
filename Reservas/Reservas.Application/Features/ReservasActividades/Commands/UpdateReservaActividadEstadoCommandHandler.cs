using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public class UpdateReservaActividadEstadoCommandHandler : IRequestHandler<UpdateReservaActividadEstadoCommand, Result<ReservaActividadDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateReservaActividadEstadoCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ReservaActividadDto>> Handle(UpdateReservaActividadEstadoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reserva = await _unitOfWork.ReservasActividades.GetByIdAsync(request.ReservaActividadId, cancellationToken);

            if (reserva == null)
            {
                return Result<ReservaActividadDto>.Failure($"Reserva de actividad con ID {request.ReservaActividadId} no encontrada.");
            }

            var estado = await _unitOfWork.EstadosReservaActividad.GetByIdAsync(request.EstadoReservaActividadId, cancellationToken);
            if (estado == null)
            {
                return Result<ReservaActividadDto>.Failure($"Estado con ID {request.EstadoReservaActividadId} no encontrado.");
            }

            reserva.EstadoReservaActividadId = request.EstadoReservaActividadId;
            reserva.Estado = estado.Nombre;

            await _unitOfWork.ReservasActividades.UpdateAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var reservaDto = _mapper.Map<ReservaActividadDto>(reserva);
            return Result<ReservaActividadDto>.Success(reservaDto);
        }
        catch (Exception ex)
        {
            return Result<ReservaActividadDto>.Failure($"Error al actualizar el estado de la reserva de actividad: {ex.Message}");
        }
    }
}