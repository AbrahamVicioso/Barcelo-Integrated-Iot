using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.RegistrosAcceso.Queries;

public class GetAllRegistrosAccesoQuery : IRequest<Result<PagedResult<RegistrosAccesoDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
