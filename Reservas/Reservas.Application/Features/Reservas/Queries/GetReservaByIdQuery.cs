using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Reservas.Queries;

public class GetReservaByIdQuery : IRequest<Result<ReservaDto>>
{
    public int ReservaId { get; set; }
}
