using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Historial.Queries;

public class GetHistorialReservasMeQuery : IRequest<Result<PagedResult<ReservaDto>>>
{
    public string UsuarioId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetHistorialActividadesMeQuery : IRequest<Result<PagedResult<ReservaActividadDto>>>
{
    public string UsuarioId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
