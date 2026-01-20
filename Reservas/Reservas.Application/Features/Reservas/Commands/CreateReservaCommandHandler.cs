using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Domain.Entites;
using Reservas.Domain.Interfaces;

namespace Reservas.Application.Features.Reservas.Commands;

public class CreateReservaCommandHandler : IRequestHandler<CreateReservaCommand, Result<ReservaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateReservaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ReservaDto>> Handle(CreateReservaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reserva = _mapper.Map<Reserva>(request);

            // Generate unique reservation number
            reserva.NumeroReserva = $"RES-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            reserva.Estado = "Pendiente";
            reserva.FechaCreacion = DateTime.UtcNow;

            await _unitOfWork.Reservas.AddAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var reservaDto = _mapper.Map<ReservaDto>(reserva);
            return Result<ReservaDto>.Success(reservaDto);
        }
        catch (Exception ex)
        {
            return Result<ReservaDto>.Failure($"Error al crear la reserva: {ex.Message}");
        }
    }
}
