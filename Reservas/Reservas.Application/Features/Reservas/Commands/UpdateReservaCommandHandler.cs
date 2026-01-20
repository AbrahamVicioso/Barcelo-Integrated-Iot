using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Domain.Interfaces;

namespace Reservas.Application.Features.Reservas.Commands;

public class UpdateReservaCommandHandler : IRequestHandler<UpdateReservaCommand, Result<ReservaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateReservaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ReservaDto>> Handle(UpdateReservaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reserva = await _unitOfWork.Reservas.GetByIdAsync(request.ReservaId, cancellationToken);

            if (reserva == null)
            {
                return Result<ReservaDto>.Failure($"Reserva con ID {request.ReservaId} no encontrada.");
            }

            _mapper.Map(request, reserva);
            reserva.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.Reservas.UpdateAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var reservaDto = _mapper.Map<ReservaDto>(reserva);
            return Result<ReservaDto>.Success(reservaDto);
        }
        catch (Exception ex)
        {
            return Result<ReservaDto>.Failure($"Error al actualizar la reserva: {ex.Message}");
        }
    }
}
