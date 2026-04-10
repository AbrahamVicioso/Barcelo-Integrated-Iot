using MediatR;
using Usuarios.Application.DTOs.Dashboard;

namespace Usuarios.Application.UseCases.Dashboard.Queries.GetPersonalStats;

public record GetPersonalStatsQuery(int? HotelId = null) : IRequest<PersonalStatsDto>;
