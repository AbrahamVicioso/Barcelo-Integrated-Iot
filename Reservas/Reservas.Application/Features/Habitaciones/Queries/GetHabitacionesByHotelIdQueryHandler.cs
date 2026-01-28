using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Habitaciones.Queries;

public class GetHabitacionesByHotelIdQueryHandler : IRequestHandler<GetHabitacionesByHotelIdQuery, Result<IEnumerable<HabitacionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetHabitacionesByHotelIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<HabitacionDto>>> Handle(GetHabitacionesByHotelIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var habitaciones = await _unitOfWork.Habitaciones.GetByHotelId(request.HotelId);
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
            return Result<IEnumerable<HabitacionDto>>.Failure($"Error al obtener las habitaciones del hotel: {ex.Message}");
        }
    }
}
