using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Habitaciones.Queries;

public class GetHabitacionesByHotelIdQuery : IRequest<Result<IEnumerable<HabitacionDto>>>
{
    public int HotelId { get; set; }
}
