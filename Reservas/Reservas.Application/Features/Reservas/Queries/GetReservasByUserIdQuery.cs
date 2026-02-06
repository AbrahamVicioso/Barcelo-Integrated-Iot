using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Reservas.Queries;

public class GetReservasByUserIdQuery : IRequest<Result<IEnumerable<ReservaDto>>>
{
    public string UserId { get; set; } = string.Empty;
}
