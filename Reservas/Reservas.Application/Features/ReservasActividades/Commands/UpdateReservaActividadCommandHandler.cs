using AutoMapper;
using MediatR;
using Notification.Domain.Events;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public class UpdateReservaActividadCommandHandler : IRequestHandler<UpdateReservaActividadCommand, Result<ReservaActividadDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IReservaActividadKafkaProducer _kafkaProducer;

    public UpdateReservaActividadCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IReservaActividadKafkaProducer kafkaProducer)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<Result<ReservaActividadDto>> Handle(UpdateReservaActividadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reserva = await _unitOfWork.ReservasActividades.GetByIdWithActividadAsync(request.ReservaActividadId, cancellationToken);

            if (reserva == null)
                return Result<ReservaActividadDto>.Failure($"Reserva de actividad con ID {request.ReservaActividadId} no encontrada.");

            var fechaAnterior = reserva.FechaReserva;
            var horaAnterior = reserva.HoraReserva;

            _mapper.Map(request, reserva);

            await _unitOfWork.ReservasActividades.UpdateAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            bool fechaCambio = reserva.FechaReserva != fechaAnterior || reserva.HoraReserva != horaAnterior;
            if (fechaCambio)
            {
                try
                {
                    var evt = new ActividadFechaActualizadaEvent
                    {
                        ReservaActividadId = reserva.ReservaActividadId,
                        ActividadId = reserva.ActividadId,
                        FechaReserva = DateTime.SpecifyKind(reserva.FechaReserva, DateTimeKind.Unspecified),
                        HoraReserva = reserva.HoraReserva,
                        DuracionMinutos = reserva.Actividad?.DuracionMinutos
                    };
                    await _kafkaProducer.PublishActividadFechaActualizadaAsync(evt, cancellationToken);
                }
                catch (Exception ex)
                {
                    // No-bloqueante: log y continuar
                    _ = ex;
                }
            }

            var reservaDto = _mapper.Map<ReservaActividadDto>(reserva);
            return Result<ReservaActividadDto>.Success(reservaDto);
        }
        catch (Exception ex)
        {
            return Result<ReservaActividadDto>.Failure($"Error al actualizar la reserva de actividad: {ex.Message}");
        }
    }
}
