using MediatR;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entites;

namespace Reservas.Application.Features.Reservas.Commands;

public class PerformCheckInCommandHandler : IRequestHandler<PerformCheckInCommand, Result<CheckInDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuariosApiService _usuariosApiService;
    private readonly IReservaKafkaProducer _kafkaProducer;
    private readonly ILogger<PerformCheckInCommandHandler> _logger;

    public PerformCheckInCommandHandler(
        IUnitOfWork unitOfWork,
        IUsuariosApiService usuariosApiService,
        IReservaKafkaProducer kafkaProducer,
        ILogger<PerformCheckInCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _usuariosApiService = usuariosApiService;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
    }

    public async Task<Result<CheckInDto>> Handle(PerformCheckInCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // RF-003: Validar numero de reserva
            var reserva = await _unitOfWork.Reservas.GetByNumeroReservaAsync(request.NumeroReserva, cancellationToken);

            if (reserva == null)
                return Result<CheckInDto>.Failure("No se encontró ninguna reserva con el número proporcionado.");

            // Verificar que la reserva esté en estado válido para check-in
            if (reserva.EstadoReservaId == EstadoReserva.Activa)
                return Result<CheckInDto>.Failure("Esta reserva ya tiene un check-in registrado.");

            if (reserva.EstadoReservaId == EstadoReserva.CheckOut)
                return Result<CheckInDto>.Failure("No se puede realizar check-in para una reserva que ya tiene checkout.");

            if (reserva.EstadoReservaId == EstadoReserva.Cancelada)
                return Result<CheckInDto>.Failure("No se puede realizar check-in para una reserva cancelada.");

            // Verificar que no exista ya un check-in para esta reserva
            if (reserva.CheckInRealizado.HasValue)
                return Result<CheckInDto>.Failure("Ya existe un check-in registrado para esta reserva.");

            // Verificar que la fecha actual esté dentro del rango de la reserva
            var hoy = DateTime.UtcNow.Date;
            if (hoy < reserva.FechaCheckIn.Date)
                return Result<CheckInDto>.Failure($"El check-in no puede realizarse antes de la fecha de entrada: {reserva.FechaCheckIn:dd/MM/yyyy}.");
            if (hoy > reserva.FechaCheckOut.Date)
                return Result<CheckInDto>.Failure($"La reserva venció el {reserva.FechaCheckOut:dd/MM/yyyy}. No es posible realizar el check-in.");

            // RF-003: Validar identidad del huesped via email
            var huesped = await _usuariosApiService.GetHuespedByIdAsync(reserva.HuespedId, cancellationToken);

            if (huesped == null)
                return Result<CheckInDto>.Failure("No se encontró el huésped asociado a esta reserva.");

            if (string.IsNullOrEmpty(huesped.CorreoElectronico) ||
                !huesped.CorreoElectronico.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
                return Result<CheckInDto>.Failure("El email proporcionado no coincide con el registrado para esta reserva.");

            var fechaCheckIn = DateTime.UtcNow;

            reserva.EstadoReservaId = EstadoReserva.Activa;
            reserva.CheckInRealizado = fechaCheckIn;

            await _unitOfWork.Reservas.UpdateAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Check-in completado para reserva {NumeroReserva}, huesped {HuespedId}.",
                reserva.NumeroReserva, reserva.HuespedId);

            // Recolectar todos los HuespedIds únicos (principal + adicionales)
            var huespedIds = reserva.ReservaHuespedes
                .Select(rh => rh.HuespedId)
                .Append(reserva.HuespedId)
                .Distinct()
                .ToList();

            // Obtener datos de cada huésped para incluir email en el evento
            var huespedInfos = new List<HuespedCheckInInfo>();
            foreach (var huespedId in huespedIds)
            {
                try
                {
                    var h = await _usuariosApiService.GetHuespedByIdAsync(huespedId, cancellationToken);
                    huespedInfos.Add(new HuespedCheckInInfo
                    {
                        HuespedId = huespedId,
                        Email = h?.CorreoElectronico ?? string.Empty,
                        NombreCompleto = h?.NombreCompleto ?? string.Empty
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo obtener email del huésped {HuespedId}", huespedId);
                    huespedInfos.Add(new HuespedCheckInInfo { HuespedId = huespedId });
                }
            }

            var checkInEvent = new CheckInRealizadoEvent
            {
                ReservaId = reserva.ReservaId,
                NumeroReserva = reserva.NumeroReserva,
                Huespedes = huespedInfos,
                FechaCheckIn = fechaCheckIn,
                FechaCheckOut = reserva.FechaCheckOut
            };

            await _kafkaProducer.PublishCheckInRealizadoAsync(checkInEvent, cancellationToken);

            _logger.LogInformation(
                "CheckInRealizadoEvent publicado para reserva {NumeroReserva}, {Count} huespedes.",
                reserva.NumeroReserva, huespedIds.Count);

            return Result<CheckInDto>.Success(new CheckInDto
            {
                ReservaId = reserva.ReservaId,
                FechaCheckIn = reserva.CheckInRealizado.Value
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al realizar check-in para reserva {NumeroReserva}", request.NumeroReserva);
            return Result<CheckInDto>.Failure($"Error al realizar el check-in: {ex.Message}");
        }
    }

}
