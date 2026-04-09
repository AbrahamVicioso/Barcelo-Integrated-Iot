using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.Dispositivos.Queries;

public class GetAllDispositivosQuery : IRequest<Result<PagedResult<DispositivoDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
