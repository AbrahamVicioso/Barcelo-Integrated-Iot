using MediatR;
using Dispositivos.Application.Common;

namespace Dispositivos.Application.Features.Dispositivos.Commands;

public class DeleteDispositivoCommand : IRequest<Result<bool>>
{
    public Guid DispositivoId { get; set; }
}
