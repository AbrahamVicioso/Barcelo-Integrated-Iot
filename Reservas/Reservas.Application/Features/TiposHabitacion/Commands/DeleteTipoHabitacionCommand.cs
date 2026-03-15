using MediatR;
using Reservas.Application.Common;

namespace Reservas.Application.Features.TiposHabitacion.Commands;

public class DeleteTipoHabitacionCommand : IRequest<Result<bool>>
{
    public int TipoHabitacionId { get; set; }
}
