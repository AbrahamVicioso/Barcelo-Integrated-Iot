using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.EstadosReservaActividad.Queries;

public class GetAllEstadosReservaActividadQuery : IRequest<Result<IEnumerable<EstadoReservaActividadDto>>>
{
}