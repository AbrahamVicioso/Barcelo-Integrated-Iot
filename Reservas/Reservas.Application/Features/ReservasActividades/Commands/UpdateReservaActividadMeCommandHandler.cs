using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public class UpdateReservaActividadMeCommandHandler : IRequestHandler<UpdateReservaActividadMeCommand, Result<ReservaActividadDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHuespedRepository _huespedRepository;
    private readonly IMapper _mapper;

    public UpdateReservaActividadMeCommandHandler(
        IUnitOfWork unitOfWork,
        IHuespedRepository huespedRepository,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _huespedRepository = huespedRepository;
        _mapper = mapper;
    }

    public async Task<Result<ReservaActividadDto>> Handle(UpdateReservaActividadMeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var huespedId = await _huespedRepository.GetHuespedIdByUserIdAsync(request.UsuarioId, cancellationToken);
            if (huespedId == null)
            {
                return Result<ReservaActividadDto>.NotFound("Huésped no encontrado para este usuario.");
            }

            var reserva = await _unitOfWork.ReservasActividades.GetByIdAsync(request.ReservaActividadId, cancellationToken);
            if (reserva == null)
            {
                return Result<ReservaActividadDto>.NotFound($"Reserva de actividad con ID {request.ReservaActividadId} no encontrada.");
            }

            if (reserva.HuespedId != huespedId.Value)
            {
                return Result<ReservaActividadDto>.Failure("No tienes permiso para editar esta reserva.");
            }

            reserva.FechaReserva = request.FechaReserva;
            reserva.HoraReserva = request.HoraReserva;
            reserva.NumeroPersonas = request.NumeroPersonas;
            reserva.MontoTotal = request.MontoTotal;
            reserva.NotasEspeciales = request.NotasEspeciales;

            await _unitOfWork.ReservasActividades.UpdateAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var reservaDto = _mapper.Map<ReservaActividadDto>(reserva);
            return Result<ReservaActividadDto>.Success(reservaDto);
        }
        catch (Exception ex)
        {
            return Result<ReservaActividadDto>.Failure($"Error al actualizar la reserva de actividad: {ex.Message}");
        }
    }
}