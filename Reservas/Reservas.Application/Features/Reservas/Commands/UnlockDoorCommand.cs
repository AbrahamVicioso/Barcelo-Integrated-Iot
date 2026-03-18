using MediatR;
using Reservas.Application.Common;

namespace Reservas.Application.Features.Reservas.Commands;

public class UnlockDoorCommand : IRequest<Result<string>>
{
    public int ReservaId { get; set; }
}
