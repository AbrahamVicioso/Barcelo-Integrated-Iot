using MediatR;
using Dispositivos.Application.Common;

namespace Dispositivos.Application.Features.EstadosDispositivo.Commands;

public class DeleteEstadoDispositivoCommand : IRequest<Result<int>>
{
    public int EstadoDispositivoId { get; set; }
}
