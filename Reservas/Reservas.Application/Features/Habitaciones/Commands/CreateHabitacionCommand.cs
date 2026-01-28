using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Habitaciones.Commands;

public class CreateHabitacionCommand : IRequest<Result<int>>
{
    public CreateHabitacionDto Habitacion { get; set; } = new();
}
