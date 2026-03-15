using MediatR;
using Reservas.Application.Common;

namespace Reservas.Application.Features.EstadosHabitacion.Commands;

public class DeleteEstadoHabitacionCommand : IRequest<Result<int>>
{
    public int EstadoHabitacionId { get; set; }
}
