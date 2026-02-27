using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entites;
using Notification.Domain.Events;

namespace Reservas.Application.Features.Reservas.Commands;

public class CreateReservaCommandHandler : IRequestHandler<CreateReservaCommand, Result<ReservaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHuespedRepository _huespedRepository;
    private readonly IReservaKafkaProducer _kafkaProducer;
    private readonly ILogger<CreateReservaCommandHandler> _logger;

    public CreateReservaCommandHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        IHuespedRepository huespedRepository,
        IReservaKafkaProducer kafkaProducer,
        ILogger<CreateReservaCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _huespedRepository = huespedRepository;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
    }

    public async Task<Result<ReservaDto>> Handle(CreateReservaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reserva = new Reserva {
                FechaCheckIn = request.FechaCheckIn,
                FechaCheckOut = request.FechaCheckOut,
                HuespedId = request.HuespedId,
                HabitacionId = request.HabitacionId,
                MontoTotal = request.MontoTotal,
                MontoPagado = request.MontoPagado
            };

            // Generate unique reservation number
            reserva.NumeroReserva = $"RES-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            reserva.Estado = "Pendiente";
            reserva.FechaCreacion = DateTime.UtcNow;

            await _unitOfWork.Reservas.AddAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var email = await _huespedRepository.GetHuespedIdByEmail(reserva.HuespedId);

            var habitacion = await _unitOfWork.Habitaciones.GetById(reserva.HabitacionId);

            // Publish to Kafka for notification
            var reservaCreadaEvent = new ReservaCreadaEvent
            {
                Email = email,
                NumeroReserva = reserva.NumeroReserva,
                FechaCheckIn = reserva.FechaCheckIn,
                FechaCheckOut = reserva.FechaCheckOut,
                MontoTotal = request.MontoTotal,
                HabitacionNumero = $"Habitación {habitacion.NumeroHabitacion}",
                HotelNombre = habitacion.Hotel.Nombre ?? "Hotel Barcelo",
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
