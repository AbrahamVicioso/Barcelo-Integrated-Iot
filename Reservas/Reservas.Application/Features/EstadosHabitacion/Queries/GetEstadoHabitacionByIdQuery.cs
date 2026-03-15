using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.EstadosHabitacion.Queries;

public class GetEstadoHabitacionByIdQuery : IRequest<Result<EstadoHabitacionDto>>
{
    public int EstadoHabitacionId { get; set; }
}
