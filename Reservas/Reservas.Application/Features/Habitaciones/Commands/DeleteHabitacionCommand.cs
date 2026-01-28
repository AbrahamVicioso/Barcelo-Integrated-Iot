using MediatR;
using Reservas.Application.Common;

namespace Reservas.Application.Features.Habitaciones.Commands;

public class DeleteHabitacionCommand : IRequest<Result<bool>>
{
    public int HabitacionId { get; set; }
}
