using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs.Analytics;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Analytics.Queries;

public record GetOcupacionHistoricoQuery : IRequest<Result<List<OcupacionHistoricoDto>>>;

public class GetOcupacionHistoricoQueryHandler : IRequestHandler<GetOcupacionHistoricoQuery, Result<List<OcupacionHistoricoDto>>>
{
    private readonly IReservasAnalyticsService _analytics;

    public GetOcupacionHistoricoQueryHandler(IReservasAnalyticsService analytics) =>
        _analytics = analytics;

    public async Task<Result<List<OcupacionHistoricoDto>>> Handle(GetOcupacionHistoricoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return Result<List<OcupacionHistoricoDto>>.Success(await _analytics.GetOcupacionHistoricoAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            return Result<List<OcupacionHistoricoDto>>.Failure($"Error al obtener histórico de ocupación: {ex.Message}");
        }
    }
}
