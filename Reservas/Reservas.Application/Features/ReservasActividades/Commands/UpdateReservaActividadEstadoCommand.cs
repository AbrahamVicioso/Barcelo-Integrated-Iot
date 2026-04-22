using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.ReservasActividades.Commands;

public class UpdateReservaActividadEstadoCommand : IRequest<Result<ReservaActividadDto>>
{
    public int ReservaActividadId { get; set; }
    public int EstadoReservaActividadId { get; set; }
}