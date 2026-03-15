using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.TiposHabitacion.Queries;

public class GetAllTiposHabitacionQuery : IRequest<Result<IEnumerable<TipoHabitacionDto>>>
{
}
