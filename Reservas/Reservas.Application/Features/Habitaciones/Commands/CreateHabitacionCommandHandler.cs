using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entities;

namespace Reservas.Application.Features.Habitaciones.Commands;

public class CreateHabitacionCommandHandler : IRequestHandler<CreateHabitacionCommand, Result<int>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CreateHabitacionCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateHabitacionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existe = await _unitOfWork.Habitaciones.ExistsAsync(
                request.Habitacion.HotelId,
                request.Habitacion.NumeroHabitacion,
                cancellationToken);

            if (existe)
                return Result<int>.Failure($"Ya existe esta habitación en este hotel.");

            var habitacion = _mapper.Map<Habitacion>(request.Habitacion);
            habitacion.FechaCreacion = DateTime.Now;

            await _unitOfWork.Habitaciones.AddAsync(habitacion, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(habitacion.HabitacionId);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error al crear la habitación: {ex.Message}");
        }
    }
}
