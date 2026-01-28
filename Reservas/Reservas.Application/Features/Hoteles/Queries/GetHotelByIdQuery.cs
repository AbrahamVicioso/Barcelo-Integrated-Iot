using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Hoteles.Queries;

public class GetHotelByIdQuery : IRequest<Result<HotelesDto>>
{
    public int HotelId { get; set; }
}
