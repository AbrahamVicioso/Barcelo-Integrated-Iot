using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs.Analytics;
using Dispositivos.Application.Interfaces;
using MediatR;

namespace Dispositivos.Application.Features.Analytics.Queries;

public record GetAccesosResumenQuery : IRequest<Result<AccesosResumenDto>>;

public class GetAccesosResumenQueryHandler : IRequestHandler<GetAccesosResumenQuery, Result<AccesosResumenDto>>
{
    private readonly IDispositivosAnalyticsService _analytics;

    public GetAccesosResumenQueryHandler(IDispositivosAnalyticsService analytics) =>
        _analytics = analytics;

    public async Task<Result<AccesosResumenDto>> Handle(GetAccesosResumenQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return Result<AccesosResumenDto>.Success(await _analytics.GetAccesosResumenAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            return Result<AccesosResumenDto>.Failure($"Error al obtener resumen de accesos: {ex.Message}");
        }
    }
}
