using MediatR;
using Dispositivos.Application.Common;

namespace Dispositivos.Application.Features.TiposDispositivo.Commands;

public class DeleteTipoDispositivoCommand : IRequest<Result<int>>
{
    public int TipoDispositivoId { get; set; }
}
