using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.Dispositivos.Queries;

public class GetAllDispositivosQuery : IRequest<Result<IEnumerable<DispositivoDto>>>
{
}
