using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs.Analytics;
using Dispositivos.Application.Interfaces;
using MediatR;

namespace Dispositivos.Application.Features.Analytics.Queries;

public record GetIncidentesResumenQuery : IRequest<Result<IncidentesResumenDto>>;

public class GetIncidentesResumenQueryHandler : IRequestHandler<GetIncidentesResumenQuery, Result<IncidentesResumenDto>>
{
    private readonly IDispositivosAnalyticsService _analytics;

    public GetIncidentesResumenQueryHandler(IDispositivosAnalyticsService analytics) =>
        _analytics = analytics;

    public async Task<Result<IncidentesResumenDto>> Handle(GetIncidentesResumenQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return Result<IncidentesResumenDto>.Success(await _analytics.GetIncidentesResumenAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            return Result<IncidentesResumenDto>.Failure($"Error al obtener resumen de incidentes: {ex.Message}");
        }
    }
}
