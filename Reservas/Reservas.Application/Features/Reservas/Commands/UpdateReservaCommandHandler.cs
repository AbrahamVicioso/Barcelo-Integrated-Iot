using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entites;
using System.Security.Claims;

namespace Reservas.Application.Features.Reservas.Commands;

public class UpdateReservaCommandHandler : IRequestHandler<UpdateReservaCommand, Result<ReservaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateReservaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<ReservaDto>> Handle(UpdateReservaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reserva = await _unitOfWork.Reservas.GetByIdAsync(request.ReservaId, cancellationToken);

            if (reserva == null)
                return Result<ReservaDto>.Failure($"Reserva con ID {request.ReservaId} no encontrada.");

            var habitacionId = request.HabitacionId ?? reserva.HabitacionId;
            var fechaCheckIn = request.FechaCheckIn != default ? request.FechaCheckIn : reserva.FechaCheckIn;
            var fechaCheckOut = request.FechaCheckOut != default ? request.FechaCheckOut : reserva.FechaCheckOut;

            if (fechaCheckOut <= fechaCheckIn)
                return Result<ReservaDto>.Failure("La fecha de check-out debe ser posterior a la fecha de check-in.");

            bool fechaCheckInCambiada = request.FechaCheckIn != default && request.FechaCheckIn != reserva.FechaCheckIn;
            if (fechaCheckInCambiada && fechaCheckIn.ToLocalTime() < DateTime.Now)
                return Result<ReservaDto>.Failure("La fecha de check-in no puede ser en el pasado.");

            if (habitacionId.HasValue)
            {
                var ocupada = await _unitOfWork.Reservas.IsHabitacionOcupadaAsync(
                    habitacionId.Value,
                    fechaCheckIn,
                    fechaCheckOut,
                    cancellationToken,
                    excludeReservaId: request.ReservaId);

                if (ocupada)
                {
                    var habitacion = await _unitOfWork.Habitaciones.GetById(habitacionId.Value);
                    return Result<ReservaDto>.Failure(
                        $"La habitación {habitacion?.NumeroHabitacion ?? habitacionId.ToString()} ya tiene una reserva en el rango de fechas seleccionado.");
                }
            }

            var user = _httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirst("nameid")?.Value
                      ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var estadoOriginal = reserva.EstadoReservaId;
            _mapper.Map(request, reserva);
            reserva.EstadoReservaId = estadoOriginal;
            reserva.FechaActualizacion = DateTime.UtcNow;
            reserva.ModificadoPor = userId;

            if (request.Huespedes != null)
            {
                var huespedes = request.Huespedes;
                var allHuespedIds = huespedes.Select(h => h.HuespedId).ToHashSet();
                // +1 por el huesped titular de la reserva
                int totalHuespedes = allHuespedIds.Count + 1;

                int habId = request.HabitacionId ?? reserva.HabitacionId ?? 0;
                if (habId > 0)
                {
                    var habitacion = await _unitOfWork.Habitaciones.GetById(habId);
                    if (habitacion != null && habitacion.CapacidadMaxima < totalHuespedes)
                        return Result<ReservaDto>.Failure(
                            $"La habitación {habitacion.NumeroHabitacion} tiene capacidad máxima de {habitacion.CapacidadMaxima} persona(s), " +
                            $"pero la reserva incluye {totalHuespedes} huésped(es) (1 titular + {allHuespedIds.Count} adicional(es)).");
                }

                reserva.ReservaHuespedes.Clear();
                foreach (var id in allHuespedIds)
                {
                    var permisos = huespedes.FirstOrDefault(h => h.HuespedId == id);
                    reserva.ReservaHuespedes.Add(new ReservaHuesped
                    {
                        HuespedId = id,
                        PuedeCrearActividadesRecreativas = permisos?.PuedeCrearActividadesRecreativas ?? false,
                        PuedeDesbloquearCerradura = permisos?.PuedeDesbloquearCerradura ?? false,
                        FechaAgregado = DateTime.UtcNow
                    });
                }
            }
            else if (request.HabitacionId.HasValue || reserva.HabitacionId.HasValue)
            {
                int habId = request.HabitacionId ?? reserva.HabitacionId ?? 0;
                if (habId > 0)
                {
                    var habitacion = await _unitOfWork.Habitaciones.GetById(habId);
                    // +1 por el huesped titular de la reserva
                    int huespedCount = reserva.ReservaHuespedes.Count > 0
                        ? reserva.ReservaHuespedes.Count
                        : request.NumeroHuespedes;
                    int totalHuespedes = huespedCount + 1;
                    if (habitacion != null && habitacion.CapacidadMaxima < totalHuespedes)
                        return Result<ReservaDto>.Failure(
                            $"La habitación {habitacion.NumeroHabitacion} tiene capacidad máxima de {habitacion.CapacidadMaxima} persona(s), " +
                            $"pero la reserva incluye {totalHuespedes} huésped(es) (1 titular + {huespedCount} adicional(es)).");
                }
            }

            await _unitOfWork.Reservas.UpdateAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var reservaDto = _mapper.Map<ReservaDto>(reserva);
            return Result<ReservaDto>.Success(reservaDto);
        }
        catch (Exception ex)
        {
            return Result<ReservaDto>.Failure($"Error al actualizar la reserva: {ex.Message}");
        }
    }
}
