using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs.Analytics;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Analytics.Queries;

public record GetActividadesMasPopularesQuery(int Top = 5) : IRequest<Result<List<ActividadPopularDto>>>;

public class GetActividadesMasPopularesQueryHandler : IRequestHandler<GetActividadesMasPopularesQuery, Result<List<ActividadPopularDto>>>
{
    private readonly IReservasAnalyticsService _analytics;

    public GetActividadesMasPopularesQueryHandler(IReservasAnalyticsService analytics) =>
        _analytics = analytics;

    public async Task<Result<List<ActividadPopularDto>>> Handle(GetActividadesMasPopularesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var top = request.Top > 0 ? request.Top : 5;
            return Result<List<ActividadPopularDto>>.Success(await _analytics.GetActividadesMasPopularesAsync(top, cancellationToken));
        }
        catch (Exception ex)
        {
            return Result<List<ActividadPopularDto>>.Failure($"Error al obtener actividades más populares: {ex.Message}");
        }
    }
}
