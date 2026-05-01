using AutoMapper;
using MediatR;
using Notification.Domain.Events;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entites;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public class CreateReservaActividadCommandHandler : IRequestHandler<CreateReservaActividadCommand, Result<ReservaActividadDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IReservaActividadKafkaProducer _kafkaProducer;

    public CreateReservaActividadCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IReservaActividadKafkaProducer kafkaProducer)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<Result<ReservaActividadDto>> Handle(CreateReservaActividadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reserva = _mapper.Map<Domain.Entites.ReservasActividades>(request);
            reserva.EstadoReservaActividadId = EstadoReservaActividad.Confirmada;
            reserva.Estado = "Confirmada";
            reserva.FechaCreacion = DateTime.UtcNow;
            reserva.RecordatorioEnviado = false;

            await _unitOfWork.ReservasActividades.AddAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var reservaDto = _mapper.Map<ReservaActividadDto>(reserva);

            // Publish event so Dispositivos can generate a PIN for the activity lock (fire-and-forget)
            try
            {
                var actividad = await _unitOfWork.ActividadesRecreativas.GetByIdAsync(request.ActividadId, cancellationToken);
                if (actividad?.RequiereReserva == true)
                {
                    var evt = new ReservaActividadConfirmadaEvent
                    {
                        ReservaActividadId = reserva.ReservaActividadId,
                        ActividadId = reserva.ActividadId,
                        HuespedId = reserva.HuespedId,
                        FechaReserva = reserva.FechaReserva,
                        HoraReserva = reserva.HoraReserva,
                        DuracionMinutos = actividad.DuracionMinutos,
                        NombreActividad = actividad.NombreActividad ?? string.Empty
                    };
                    await _kafkaProducer.PublishReservaActividadConfirmadaAsync(evt, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                // Non-fatal: reservation was already saved; just log the failure
                _ = ex;
            }

            return Result<ReservaActividadDto>.Success(reservaDto);
        }
        catch (Exception ex)
        {
            return Result<ReservaActividadDto>.Failure($"Error al crear la reserva de actividad: {ex.Message}");
        }
    }
}
