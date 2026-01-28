using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Hoteles.Commands;

public class UpdateHotelCommandHandler : IRequestHandler<UpdateHotelCommand, Result<HotelesDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateHotelCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<HotelesDto>> Handle(UpdateHotelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var hotel = await _unitOfWork.Hoteles.GetById(request.HotelId);

            if (hotel == null)
            {
                return Result<HotelesDto>.Failure($"Hotel con ID {request.HotelId} no encontrado.");
            }

            _mapper.Map(request, hotel);

            await _unitOfWork.Hoteles.UpdateAsync(hotel, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var hotelDto = _mapper.Map<HotelesDto>(hotel);
            return Result<HotelesDto>.Success(hotelDto);
        }
        catch (Exception ex)
        {
            return Result<HotelesDto>.Failure($"Error al actualizar el hotel: {ex.Message}");
        }
    }
}
