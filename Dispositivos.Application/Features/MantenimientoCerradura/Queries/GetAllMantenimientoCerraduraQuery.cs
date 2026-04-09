using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.MantenimientoCerradura.Queries;

public class GetAllMantenimientoCerraduraQuery : IRequest<Result<PagedResult<MantenimientoCerraduraDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
