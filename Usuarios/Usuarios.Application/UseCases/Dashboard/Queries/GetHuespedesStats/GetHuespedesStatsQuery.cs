using MediatR;
using Usuarios.Application.DTOs.Dashboard;

namespace Usuarios.Application.UseCases.Dashboard.Queries.GetHuespedesStats;

public record GetHuespedesStatsQuery(int? HotelId = null) : IRequest<HuespedesStatsDto>;
