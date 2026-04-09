using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.CredencialesAcceso.Queries;

public class GetCredencialesByHuespedIdQuery : IRequest<Result<PagedResult<CredencialesAccesoDto>>>
{
    public int HuespedId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
