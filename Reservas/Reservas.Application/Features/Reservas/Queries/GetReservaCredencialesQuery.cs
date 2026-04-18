using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Reservas.Queries;

public class GetReservaCredencialesQuery : IRequest<Result<IEnumerable<CredencialHuespedDto>>>
{
    public int ReservaId { get; set; }
    public string UserId { get; set; }
}
