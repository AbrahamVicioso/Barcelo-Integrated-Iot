using MediatR;
using Reservas.Application.Common;

namespace Reservas.Application.Features.Reservas.Commands;

public class DeleteReservaCommand : IRequest<Result<bool>>
{
    public int ReservaId { get; set; }
}
