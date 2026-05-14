using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entities;

namespace Reservas.Application.Features.Hoteles.Commands;

public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, Result<int>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CreateHotelCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
    {
        if (request.Hotel.NumeroHabitaciones <= 0)
            return Result<int>.Failure("El número de habitaciones debe ser mayor a 0.");

        try
        {
            var hotel = _mapper.Map<Hotel>(request.Hotel);
            hotel.EstaActivo = true;
            hotel.FechaCreacion = DateTime.Now;

            await _unitOfWork.Hoteles.AddAsync(hotel, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(hotel.HotelId);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error al crear el hotel: {ex.Message}");
        }
    }
}
