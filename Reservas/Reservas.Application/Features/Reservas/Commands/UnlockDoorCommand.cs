using MediatR;
using Reservas.Application.Common;

namespace Reservas.Application.Features.Reservas.Commands;

public class UnlockDoorCommand : IRequest<Result<string>>
{
    public int ReservaId { get; set; }
    public string? Pin { get; set; }
}
