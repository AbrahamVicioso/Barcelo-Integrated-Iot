using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;
using static Reservas.Domain.Entites.EstadoReserva;

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

            if (reserva.EstadoReservaId != Activa)
                return Result<ReservaDto>.Failure("Solo se puede realizar checkout de una reserva activa (con check-in completado).");

            reserva.EstadoReservaId = CheckOut;
            reserva.CheckOutRealizado = DateTime.UtcNow;
            reserva.FechaActualizacion = DateTime.UtcNow;

            await _unitOfWork.Reservas.UpdateAsync(reserva, cancellationToken);
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
