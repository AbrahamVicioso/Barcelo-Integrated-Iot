using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Reservas.Queries;

public class GetReservasByHuespedIdQuery : IRequest<Result<IEnumerable<ReservaDto>>>
{
    public int HuespedId { get; set; }
}
