using MediatR;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Features.Dashboard.Queries;

public class GetDashboardDispositivosStatsQuery : IRequest<Result<DashboardDispositivosStatsDto>>
{
    public int? HotelId { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
}
