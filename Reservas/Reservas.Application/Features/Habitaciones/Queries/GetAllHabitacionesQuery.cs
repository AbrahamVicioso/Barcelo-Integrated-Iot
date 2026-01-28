using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Habitaciones.Queries;

public class GetAllHabitacionesQuery : IRequest<Result<IEnumerable<HabitacionDto>>>
{
}
