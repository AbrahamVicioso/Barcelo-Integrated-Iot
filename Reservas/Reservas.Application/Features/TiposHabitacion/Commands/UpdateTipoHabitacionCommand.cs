using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.TiposHabitacion.Commands;

public class UpdateTipoHabitacionCommand : IRequest<Result<TipoHabitacionDto>>
{
    public int TipoHabitacionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
