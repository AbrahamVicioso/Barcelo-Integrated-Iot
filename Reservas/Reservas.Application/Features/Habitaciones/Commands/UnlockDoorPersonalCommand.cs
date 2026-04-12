using MediatR;
using Reservas.Application.Common;

namespace Reservas.Application.Features.Habitaciones.Commands;

public class UnlockDoorPersonalCommand : IRequest<Result<string>>
{
    public int HabitacionId { get; set; }
}
