using AutoMapper;
using MediatR;
using Notification.Domain.Events;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entites;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public class CreateReservaActividadMeCommandHandler : IRequestHandler<CreateReservaActividadMeCommand, Result<ReservaActividadDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHuespedRepository _huespedRepository;
    private readonly IMapper _mapper;
    private readonly IReservaActividadKafkaProducer _kafkaProducer;

    public CreateReservaActividadMeCommandHandler(
        IUnitOfWork unitOfWork,
        IHuespedRepository huespedRepository,
        IMapper mapper,
        IReservaActividadKafkaProducer kafkaProducer)
    {
        _unitOfWork = unitOfWork;
        _huespedRepository = huespedRepository;
        _mapper = mapper;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<Result<ReservaActividadDto>> Handle(CreateReservaActividadMeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var huespedId = await _huespedRepository.GetHuespedIdByUserIdAsync(request.UsuarioId, cancellationToken);
            if (huespedId == null)
                return Result<ReservaActividadDto>.NotFound("Huésped no encontrado para este usuario.");

            var reserva = new Domain.Entites.ReservasActividades
            {
                ActividadId = request.ActividadId,
                HuespedId = huespedId.Value,
                FechaReserva = request.FechaReserva,
                HoraReserva = request.HoraReserva,
                NumeroPersonas = request.NumeroPersonas,
                MontoTotal = request.MontoTotal,
                NotasEspeciales = request.NotasEspeciales,
                EstadoReservaActividadId = EstadoReservaActividad.Confirmada,
                Estado = "Confirmada",
                FechaCreacion = DateTime.Now,
                RecordatorioEnviado = false
            };

            await _unitOfWork.ReservasActividades.AddAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var reservaDto = _mapper.Map<ReservaActividadDto>(reserva);
            reservaDto.Estado = "Confirmada";

            // Publish event so Dispositivos can generate a PIN for the activity lock (fire-and-forget)
            try
            {
                var actividad = await _unitOfWork.ActividadesRecreativas.GetByIdAsync(request.ActividadId, cancellationToken);
                if (actividad?.RequiereReserva == true)
                {
                    string? email = null;
                    string? nombreCompleto = null;
                    try
                    {
                        var huespedInfo = await _huespedRepository.GetHuespedEmailYNombreAsync(huespedId.Value, cancellationToken);
                        email = huespedInfo?.Email;
                        nombreCompleto = huespedInfo?.NombreCompleto;
                    }
                    catch { /* non-fatal: event published without email */ }

                    var evt = new ReservaActividadConfirmadaEvent
                    {
                        ReservaActividadId = reserva.ReservaActividadId,
                        ActividadId = reserva.ActividadId,
                        HuespedId = reserva.HuespedId,
                        Email = email,
                        NombreCompleto = nombreCompleto,
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
                // Non-fatal: reservation was already saved
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
