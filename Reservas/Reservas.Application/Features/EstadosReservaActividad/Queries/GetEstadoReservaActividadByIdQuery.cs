using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.EstadosReservaActividad.Queries;

public class GetEstadoReservaActividadByIdQuery : IRequest<Result<EstadoReservaActividadDto>>
{
    public int EstadoReservaActividadId { get; set; }
}