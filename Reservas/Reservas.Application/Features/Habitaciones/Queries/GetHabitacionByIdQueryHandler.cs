using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Habitaciones.Queries;

public class GetHabitacionByIdQueryHandler : IRequestHandler<GetHabitacionByIdQuery, Result<HabitacionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetHabitacionByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<HabitacionDto>> Handle(GetHabitacionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var habitacion = await _unitOfWork.Habitaciones.GetById(request.HabitacionId);

            if (habitacion == null)
            {
                return Result<HabitacionDto>.Failure($"Habitación con ID {request.HabitacionId} no encontrada.");
            }

            var habitacionDto = _mapper.Map<HabitacionDto>(habitacion);
            habitacionDto.NombreHotel = habitacion.Hotel?.Nombre;
            return Result<HabitacionDto>.Success(habitacionDto);
        }
        catch (Exception ex)
        {
            return Result<HabitacionDto>.Failure($"Error al obtener la habitación: {ex.Message}");
        }
    }
}
