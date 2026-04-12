using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entites;
using static Reservas.Domain.Entites.EstadoReserva;
using Notification.Domain.Events;
using System.Security.Claims;

namespace Reservas.Application.Features.Reservas.Commands;

public class CreateReservaCommandHandler : IRequestHandler<CreateReservaCommand, Result<ReservaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHuespedRepository _huespedRepository;
    private readonly IReservaKafkaProducer _kafkaProducer;
    private readonly ILogger<CreateReservaCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateReservaCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IHuespedRepository huespedRepository,
        IReservaKafkaProducer kafkaProducer,
        ILogger<CreateReservaCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _huespedRepository = huespedRepository;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<ReservaDto>> Handle(CreateReservaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirst("nameid")?.Value
                      ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Result<ReservaDto>.Failure("No se pudo identificar al usuario autenticado.");

            if (request.FechaCheckOut <= request.FechaCheckIn)
                return Result<ReservaDto>.Failure("La fecha de check-out debe ser posterior a la fecha de check-in.");
            
            if (request.FechaCheckIn.ToLocalTime() < DateTime.Now)
                return Result<ReservaDto>.Failure("La fecha de check-in no puede ser en el pasado.");

            // Build guest list (always include the titular guest)
            var huespedes = request.Huespedes ?? [];
            var allHuespedIds = huespedes.Select(h => h.HuespedId).ToHashSet();
            // +1 por el huesped titular de la reserva
            int totalHuespedes = allHuespedIds.Count + 1;

            string habitacionNumero = "Por asignar";
            string hotelNombre = "Hotel Barcelo";

            if (request.HabitacionId.HasValue)
            {
                var habitacion = await _unitOfWork.Habitaciones.GetById(request.HabitacionId.Value);
                if (habitacion == null)
                    return Result<ReservaDto>.Failure($"Habitación con ID {request.HabitacionId.Value} no encontrada.");

                if (habitacion.CapacidadMaxima < totalHuespedes)
                    return Result<ReservaDto>.Failure(
                        $"La habitación {habitacion.NumeroHabitacion} tiene capacidad máxima de {habitacion.CapacidadMaxima} persona(s), " +
                        $"pero la reserva incluye {totalHuespedes} huésped(es) (1 titular + {allHuespedIds.Count} adicional(es)).");

                var ocupada = await _unitOfWork.Reservas.IsHabitacionOcupadaAsync(
                    request.HabitacionId.Value,
                    request.FechaCheckIn,
                    request.FechaCheckOut,
                    cancellationToken);

                if (ocupada)
                    return Result<ReservaDto>.Failure(
                        $"La habitación {habitacion.NumeroHabitacion} ya tiene una reserva en el rango de fechas seleccionado.");

                habitacionNumero = $"Habitación {habitacion.NumeroHabitacion}";
                hotelNombre = habitacion.Hotel?.Nombre ?? "Hotel Barcelo";
            }

            var reservaHuespedes = allHuespedIds.Select(id =>
            {
                var permisos = huespedes.FirstOrDefault(h => h.HuespedId == id);
                return new ReservaHuesped
                {
                    HuespedId = id,
                    PuedeCrearActividadesRecreativas = permisos?.PuedeCrearActividadesRecreativas ?? false,
                    PuedeDesbloquearCerradura = permisos?.PuedeDesbloquearCerradura ?? false,
                    FechaAgregado = DateTime.UtcNow
                };
            }).ToList();

            var reserva = new Reserva {
                FechaCheckIn = request.FechaCheckIn,
                FechaCheckOut = request.FechaCheckOut,
                HuespedId = request.HuespedId,
                HabitacionId = request.HabitacionId,
                NumeroHuespedes = request.NumeroHuespedes,
                NumeroNinos = request.NumeroNinos,
                MontoTotal = request.MontoTotal,
                MontoPagado = request.MontoPagado,
                CreadoPor = userId,
                Observaciones = request.Observaciones,
                ReservaHuespedes = reservaHuespedes
            };

            // Generate unique reservation number
            reserva.NumeroReserva = $"RES-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            reserva.EstadoReservaId = Pendiente;
            reserva.FechaCreacion = DateTime.UtcNow;

            await _unitOfWork.Reservas.AddAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var email = await _huespedRepository.GetHuespedIdByEmail(reserva.HuespedId);

            // Publish to Kafka for notification
            var reservaCreadaEvent = new ReservaCreadaEvent
            {
                Email = email,
                NumeroReserva = reserva.NumeroReserva,
                FechaCheckIn = reserva.FechaCheckIn,
                FechaCheckOut = reserva.FechaCheckOut,
                MontoTotal = request.MontoTotal,
                HabitacionNumero = habitacionNumero,
                HotelNombre = hotelNombre,
                CreatedAt = DateTime.UtcNow
            };

            await _kafkaProducer.PublishReservaCreadaAsync(reservaCreadaEvent, cancellationToken);
            _logger.LogInformation("Published ReservaCreadaEvent for reservation {NumeroReserva}", reserva.NumeroReserva);

            var reservaDto = _mapper.Map<ReservaDto>(reserva);
            return Result<ReservaDto>.Success(reservaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reservation");
            return Result<ReservaDto>.Failure($"Error al crear la reserva: {ex.Message}");
        }
    }
}
