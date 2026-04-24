using AutoMapper;
using MediatR;
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

    public CreateReservaActividadMeCommandHandler(
        IUnitOfWork unitOfWork,
        IHuespedRepository huespedRepository,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _huespedRepository = huespedRepository;
        _mapper = mapper;
    }

    public async Task<Result<ReservaActividadDto>> Handle(CreateReservaActividadMeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var huespedId = await _huespedRepository.GetHuespedIdByUserIdAsync(request.UsuarioId, cancellationToken);
            if (huespedId == null)
            {
                return Result<ReservaActividadDto>.NotFound("Huésped no encontrado para este usuario.");
            }

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
                FechaCreacion = DateTime.UtcNow,
                RecordatorioEnviado = false
            };

            await _unitOfWork.ReservasActividades.AddAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var reservaDto = _mapper.Map<ReservaActividadDto>(reserva);
            reservaDto.Estado = "Confirmada";
            return Result<ReservaActividadDto>.Success(reservaDto);
        }
        catch (Exception ex)
        {
            return Result<ReservaActividadDto>.Failure($"Error al crear la reserva de actividad: {ex.Message}");
        }
    }
}