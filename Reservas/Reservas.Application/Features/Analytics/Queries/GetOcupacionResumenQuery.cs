using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs.Analytics;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Analytics.Queries;

public record GetOcupacionResumenQuery : IRequest<Result<OcupacionResumenDto>>;

public class GetOcupacionResumenQueryHandler : IRequestHandler<GetOcupacionResumenQuery, Result<OcupacionResumenDto>>
{
    private readonly IReservasAnalyticsService _analytics;

    public GetOcupacionResumenQueryHandler(IReservasAnalyticsService analytics) =>
        _analytics = analytics;

    public async Task<Result<OcupacionResumenDto>> Handle(GetOcupacionResumenQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return Result<OcupacionResumenDto>.Success(await _analytics.GetOcupacionResumenAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            return Result<OcupacionResumenDto>.Failure($"Error al obtener resumen de ocupación: {ex.Message}");
        }
    }
}
