using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.TiposHabitacion.Commands;

public class CreateTipoHabitacionCommand : IRequest<Result<int>>
{
    public CreateTipoHabitacionDto TipoHabitacion { get; set; } = new();
}
