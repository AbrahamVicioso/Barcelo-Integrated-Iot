using MediatR;
using Dispositivos.Application.Common;

namespace Dispositivos.Application.Features.MantenimientoCerradura.Commands;

public class DeleteMantenimientoCerraduraCommand : IRequest<Result<bool>>
{
    public int MantenimientoId { get; set; }
}
