using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.EstadosHabitacion.Commands;

public class UpdateEstadoHabitacionCommand : IRequest<Result<EstadoHabitacionDto>>
{
    public int EstadoHabitacionId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
