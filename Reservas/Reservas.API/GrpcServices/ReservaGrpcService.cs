using Grpc.Contracts.Reservas;
using Grpc.Core;
using Reservas.Application.Interfaces;
using static Reservas.Domain.Entites.EstadoReserva;

namespace Reservas.API.GrpcServices;

public class ReservaGrpcService : Reserva.ReservaBase
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReservaGrpcService> _logger;

    public ReservaGrpcService(IReservaRepository reservaRepository, IUnitOfWork unitOfWork, ILogger<ReservaGrpcService> logger)
    {
        _reservaRepository = reservaRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<ReservaResponse> GetReservaActivaByHabitacionId(
        GetReservaActivaByHabitacionIdRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC: GetReservaActivaByHabitacionId para habitacionId: {HabitacionId}", request.HabitacionId);

        try
        {
            var reserva = await _reservaRepository.GetReservaActivaByHabitacionIdAsync(request.HabitacionId);

            if (reserva == null)
            {
                _logger.LogWarning("gRPC: No se encontró reserva activa para habitacionId: {HabitacionId}", request.HabitacionId);
                return new ReservaResponse { Found = false };
            }

            var huespedesAdicionales = reserva.ReservaHuespedes
                .Select(rh => rh.HuespedId)
                .ToList();

            var numeroHabitacion = string.Empty;
            if (reserva.HabitacionId.HasValue)
            {
                var habitacion = await _unitOfWork.Habitaciones.GetById(reserva.HabitacionId.Value);
                numeroHabitacion = habitacion?.NumeroHabitacion ?? string.Empty;
            }

            _logger.LogInformation(
                "gRPC: Reserva {ReservaId} encontrada para habitacionId: {HabitacionId}, HuespedId: {HuespedId}, Adicionales: {Count}",
                reserva.ReservaId, request.HabitacionId, reserva.HuespedId, huespedesAdicionales.Count);

            return new ReservaResponse
            {
                Found = true,
                ReservaId = reserva.ReservaId,
                HuespedId = reserva.HuespedId,
                HuespedesAdicionales = { huespedesAdicionales },
                FechaCheckIn = reserva.FechaCheckIn.ToString("O"),
                FechaCheckOut = reserva.FechaCheckOut.ToString("O"),
                NumeroHabitacion = numeroHabitacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC: Error al obtener reserva activa para habitacionId: {HabitacionId}", request.HabitacionId);
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}