using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.Dispositivos.Queries;

public class GetDispositivoByIdQuery : IRequest<Result<DispositivoDto>>
{
    public Guid DispositivoId { get; set; }
}
