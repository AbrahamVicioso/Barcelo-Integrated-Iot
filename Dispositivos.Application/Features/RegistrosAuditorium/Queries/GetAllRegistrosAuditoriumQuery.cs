using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.RegistrosAuditorium.Queries;

public class GetAllRegistrosAuditoriumQuery : IRequest<Result<PagedResult<RegistrosAuditoriumDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
