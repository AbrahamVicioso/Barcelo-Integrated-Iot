using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Application.Interfaces;
using Reservas.Domain.Entites;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public class CreateReservaActividadCommandHandler : IRequestHandler<CreateReservaActividadCommand, Result<ReservaActividadDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateReservaActividadCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ReservaActividadDto>> Handle(CreateReservaActividadCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reserva = _mapper.Map<Domain.Entites.ReservasActividades>(request);
            reserva.EstadoReservaActividadId = EstadoReservaActividad.Confirmada;
            reserva.Estado = "Confirmada";
            reserva.FechaCreacion = DateTime.UtcNow;
            reserva.RecordatorioEnviado = false;

            await _unitOfWork.ReservasActividades.AddAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var reservaDto = _mapper.Map<ReservaActividadDto>(reserva);
            return Result<ReservaActividadDto>.Success(reservaDto);
        }
        catch (Exception ex)
        {
            return Result<ReservaActividadDto>.Failure($"Error al crear la reserva de actividad: {ex.Message}");
        }
    }
}
