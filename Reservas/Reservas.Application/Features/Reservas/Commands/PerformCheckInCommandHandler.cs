using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entities;
using Reservas.Domain.Entites;

namespace Reservas.Application.Features.Reservas.Commands;

public class PerformCheckInCommandHandler : IRequestHandler<PerformCheckInCommand, Result<CheckInDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuariosApiService _usuariosApiService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PerformCheckInCommandHandler> _logger;

    public PerformCheckInCommandHandler(
        IUnitOfWork unitOfWork,
        IUsuariosApiService usuariosApiService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PerformCheckInCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _usuariosApiService = usuariosApiService;
        _httpContextAccessor = httpContextAccessor;
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
            var checkInExistente = await _unitOfWork.CheckIns.ExisteCheckInParaReservaAsync(reserva.ReservaId, cancellationToken);
            if (checkInExistente)
                return Result<CheckInDto>.Failure("Ya existe un check-in registrado para esta reserva.");

            // RF-003: Validar identidad del huesped via email
            var huesped = await _usuariosApiService.GetHuespedByIdAsync(reserva.HuespedId, cancellationToken);

            if (huesped == null)
                return Result<CheckInDto>.Failure("No se encontró el huésped asociado a esta reserva.");

            if (string.IsNullOrEmpty(huesped.CorreoElectronico) ||
                !huesped.CorreoElectronico.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
                return Result<CheckInDto>.Failure("El email proporcionado no coincide con el registrado para esta reserva.");

            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            var checkIn = new CheckIn
            {
                ReservaId = reserva.ReservaId,
                NombreCompleto = request.NombreCompleto,
                Email = request.Email,
                Telefono = request.Telefono,
                FechaCheckIn = DateTime.UtcNow,
                DireccionIp = ipAddress ?? string.Empty,
                Estado = "Completado"
            };

            await _unitOfWork.CheckIns.AddAsync(checkIn, cancellationToken);

            // Activar la reserva
            reserva.EstadoReservaId = EstadoReserva.Activa;
            reserva.CheckInRealizado = DateTime.UtcNow;
            await _unitOfWork.Reservas.UpdateAsync(reserva, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Check-in completado para reserva {NumeroReserva}, huesped {HuespedId}.",
                reserva.NumeroReserva, reserva.HuespedId);

            return Result<CheckInDto>.Success(new CheckInDto
            {
                CheckInId = checkIn.CheckInId,
                ReservaId = checkIn.ReservaId,
                NombreCompleto = checkIn.NombreCompleto,
                Email = checkIn.Email,
                Telefono = checkIn.Telefono,
                FechaCheckIn = checkIn.FechaCheckIn,
                Estado = checkIn.Estado
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al realizar check-in para reserva {NumeroReserva}", request.NumeroReserva);
            return Result<CheckInDto>.Failure($"Error al realizar el check-in: {ex.Message}");
        }
    }

}
