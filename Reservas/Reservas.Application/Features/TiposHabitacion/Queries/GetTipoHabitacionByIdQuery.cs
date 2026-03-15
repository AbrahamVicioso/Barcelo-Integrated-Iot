using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.TiposHabitacion.Queries;

public class GetTipoHabitacionByIdQuery : IRequest<Result<TipoHabitacionDto>>
{
    public int TipoHabitacionId { get; set; }
}
