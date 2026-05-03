using AutoMapper;
using MediatR;
using Notification.Domain.Events;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entites;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public class UpdateReservaActividadEstadoCommandHandler : IRequestHandler<UpdateReservaActividadEstadoCommand, Result<ReservaActividadDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IReservaActividadKafkaProducer _kafkaProducer;

    public UpdateReservaActividadEstadoCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IReservaActividadKafkaProducer kafkaProducer)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<Result<ReservaActividadDto>> Handle(UpdateReservaActividadEstadoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reserva = await _unitOfWork.ReservasActividades.GetByIdAsync(request.ReservaActividadId, cancellationToken);

            if (reserva == null)
            {
                return Result<ReservaActividadDto>.Failure($"Reserva de actividad con ID {request.ReservaActividadId} no encontrada.");
            }

            var estado = await _unitOfWork.EstadosReservaActividad.GetByIdAsync(request.EstadoReservaActividadId, cancellationToken);
            if (estado == null)
            {
                return Result<ReservaActividadDto>.Failure($"Estado con ID {request.EstadoReservaActividadId} no encontrado.");
            }

            reserva.EstadoReservaActividadId = request.EstadoReservaActividadId;
            reserva.Estado = estado.Nombre;

            await _unitOfWork.ReservasActividades.UpdateAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // When state changes to Confirmada, generate a credential for the activity lock
            if (request.EstadoReservaActividadId == EstadoReservaActividad.Confirmada)
            {
                try
                {
                    var actividad = await _unitOfWork.ActividadesRecreativas.GetByIdAsync(reserva.ActividadId, cancellationToken);
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
                catch
                {
                    // Non-fatal: state was already updated; credential generation is best-effort
                }
            }

            var reservaDto = _mapper.Map<ReservaActividadDto>(reserva);
            return Result<ReservaActividadDto>.Success(reservaDto);
        }
        catch (Exception ex)
        {
            return Result<ReservaActividadDto>.Failure($"Error al actualizar el estado de la reserva de actividad: {ex.Message}");
        }
    }
}