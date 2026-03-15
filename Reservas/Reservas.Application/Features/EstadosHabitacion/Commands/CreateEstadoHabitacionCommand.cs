using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.EstadosHabitacion.Commands;

public class CreateEstadoHabitacionCommand : IRequest<Result<int>>
{
    public CreateEstadoHabitacionDto EstadoHabitacion { get; set; } = new();
}
