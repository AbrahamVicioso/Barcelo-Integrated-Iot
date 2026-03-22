using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entities;
using Reservas.Domain.Entites;

namespace Reservas.Application.Features.Reservas.Commands;

public class PerformCheckOutCommandHandler : IRequestHandler<PerformCheckOutCommand, Result<ReservaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PerformCheckOutCommandHandler> _logger;

    public PerformCheckOutCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<PerformCheckOutCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ReservaDto>> Handle(PerformCheckOutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reserva = await _unitOfWork.Reservas.GetByIdAsync(request.ReservaId, cancellationToken);

            if (reserva == null)
                return Result<ReservaDto>.Failure($"No se encontró la reserva con ID {request.ReservaId}.");

            if (reserva.EstadoReservaId != EstadoReserva.Activa)
                return Result<ReservaDto>.Failure("Solo se puede realizar checkout de una reserva activa (con check-in completado).");

            var fechaCheckOut = DateTime.UtcNow;

            reserva.EstadoReservaId = EstadoReserva.CheckOut;
            reserva.CheckOutRealizado = fechaCheckOut;
            reserva.FechaActualizacion = fechaCheckOut;

            var checkOut = new Domain.Entities.CheckOut
            {
                ReservaId = reserva.ReservaId,
                FechaCheckOut = fechaCheckOut
            };

            await _unitOfWork.Reservas.UpdateAsync(reserva, cancellationToken);
            await _unitOfWork.CheckOuts.AddAsync(checkOut, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Check-out completado para reserva {ReservaId}, numero {NumeroReserva}.",
                reserva.ReservaId, reserva.NumeroReserva);

            var reservaDto = _mapper.Map<ReservaDto>(reserva);
            return Result<ReservaDto>.Success(reservaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al realizar check-out para reserva {ReservaId}", request.ReservaId);
            return Result<ReservaDto>.Failure($"Error al realizar el check-out: {ex.Message}");
        }
    }
}
