using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.TiposDispositivo.Queries;

public class GetAllTiposDispositivoQuery : IRequest<Result<PagedResult<TipoDispositivoDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
