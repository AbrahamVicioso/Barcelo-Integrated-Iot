using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Habitaciones.Queries;

public class GetAllHabitacionesQueryHandler : IRequestHandler<GetAllHabitacionesQuery, Result<IEnumerable<HabitacionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllHabitacionesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<HabitacionDto>>> Handle(GetAllHabitacionesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var habitaciones = await _unitOfWork.Habitaciones.GetAll();
            var habitacionesDto = habitaciones.Select(h =>
            {
                var dto = _mapper.Map<HabitacionDto>(h);
                dto.NombreHotel = h.Hotel?.Nombre;
                return dto;
            });
            return Result<IEnumerable<HabitacionDto>>.Success(habitacionesDto);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<HabitacionDto>>.Failure($"Error al obtener las habitaciones: {ex.Message}");
        }
    }
}
