using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.EstadosHabitacion.Queries;

public class GetAllEstadosHabitacionQuery : IRequest<Result<IEnumerable<EstadoHabitacionDto>>>
{
}
