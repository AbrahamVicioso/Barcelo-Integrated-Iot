using MediatR;
using Reservas.Application.Common;

namespace Reservas.Application.Features.Hoteles.Commands;

public class DeleteHotelCommand : IRequest<Result<bool>>
{
    public int HotelId { get; set; }
}
