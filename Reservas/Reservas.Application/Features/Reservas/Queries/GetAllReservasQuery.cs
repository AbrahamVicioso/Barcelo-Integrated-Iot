using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Reservas.Queries;

public class GetAllReservasQuery : IRequest<Result<IEnumerable<ReservaDto>>>
{
}
