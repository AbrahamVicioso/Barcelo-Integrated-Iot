using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.EstadosDispositivo.Queries;

public class GetAllEstadosDispositivoQuery : IRequest<Result<PagedResult<EstadoDispositivoDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
