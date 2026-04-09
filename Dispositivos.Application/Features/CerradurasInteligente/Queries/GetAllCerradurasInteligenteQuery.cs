using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.CerradurasInteligente.Queries;

public class GetAllCerradurasInteligenteQuery : IRequest<Result<PagedResult<CerradurasInteligenteDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
