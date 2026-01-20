using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Domain.Interfaces;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public class UpdateReservaActividadCommandHandler : IRequestHandler<UpdateReservaActividadCommand, Result<ReservaActividadDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateReservaActividadCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ReservaActividadDto>> Handle(UpdateReservaActividadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reserva = await _unitOfWork.ReservasActividades.GetByIdAsync(request.ReservaActividadId, cancellationToken);

            if (reserva == null)
            {
                return Result<ReservaActividadDto>.Failure($"Reserva de actividad con ID {request.ReservaActividadId} no encontrada.");
            }

            _mapper.Map(request, reserva);

            await _unitOfWork.ReservasActividades.UpdateAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var reservaDto = _mapper.Map<ReservaActividadDto>(reserva);
            return Result<ReservaActividadDto>.Success(reservaDto);
        }
        catch (Exception ex)
        {
            return Result<ReservaActividadDto>.Failure($"Error al actualizar la reserva de actividad: {ex.Message}");
        }
    }
}
