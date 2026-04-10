using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;

namespace Reservas.Application.Features.Dashboard.Queries;

public class GetDashboardReservasStatsQuery : IRequest<Result<DashboardReservasStatsDto>>
{
    public int? HotelId { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
}
