using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.CredencialesAcceso.Queries;

public class GetCredencialesMeHuespedQuery : IRequest<Result<PagedResult<CredencialesAccesoDto>>>
{
    public string UsuarioId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
