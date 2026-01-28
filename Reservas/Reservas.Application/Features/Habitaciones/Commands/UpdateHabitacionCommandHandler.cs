using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Habitaciones.Commands;

public class UpdateHabitacionCommandHandler : IRequestHandler<UpdateHabitacionCommand, Result<HabitacionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateHabitacionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<HabitacionDto>> Handle(UpdateHabitacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var habitacion = await _unitOfWork.Habitaciones.GetById(request.HabitacionId);

            if (habitacion == null)
            {
                return Result<HabitacionDto>.Failure($"Habitación con ID {request.HabitacionId} no encontrada.");
            }

            _mapper.Map(request, habitacion);
            habitacion.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.Habitaciones.UpdateAsync(habitacion, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var habitacionDto = _mapper.Map<HabitacionDto>(habitacion);
            return Result<HabitacionDto>.Success(habitacionDto);
        }
        catch (Exception ex)
        {
            return Result<HabitacionDto>.Failure($"Error al actualizar la habitación: {ex.Message}");
        }
    }
}
