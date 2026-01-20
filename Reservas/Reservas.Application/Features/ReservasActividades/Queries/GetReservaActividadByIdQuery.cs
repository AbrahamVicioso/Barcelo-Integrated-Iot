using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.ReservasActividades.Queries;

public class GetReservaActividadByIdQuery : IRequest<Result<ReservaActividadDto>>
{
    public int ReservaActividadId { get; set; }
}
