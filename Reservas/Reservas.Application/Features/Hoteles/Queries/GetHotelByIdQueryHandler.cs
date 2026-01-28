using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Hoteles.Queries;

public class GetHotelByIdQueryHandler : IRequestHandler<GetHotelByIdQuery, Result<HotelesDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetHotelByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<HotelesDto>> Handle(GetHotelByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var hotel = await _unitOfWork.Hoteles.GetById(request.HotelId);

            if (hotel == null)
            {
                return Result<HotelesDto>.Failure($"Hotel con ID {request.HotelId} no encontrado.");
            }

            var hotelDto = _mapper.Map<HotelesDto>(hotel);
            return Result<HotelesDto>.Success(hotelDto);
        }
        catch (Exception ex)
        {
            return Result<HotelesDto>.Failure($"Error al obtener el hotel: {ex.Message}");
        }
    }
}
