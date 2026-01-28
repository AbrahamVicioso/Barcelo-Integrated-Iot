using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Habitaciones.Queries;

public class GetHabitacionByIdQuery : IRequest<Result<HabitacionDto>>
{
    public int HabitacionId { get; set; }
}
