using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs.Analytics;
using Dispositivos.Application.Interfaces;
using MediatR;

namespace Dispositivos.Application.Features.Analytics.Queries;

public record GetDispositivosResumenQuery : IRequest<Result<DispositivosResumenDto>>;

public class GetDispositivosResumenQueryHandler : IRequestHandler<GetDispositivosResumenQuery, Result<DispositivosResumenDto>>
{
    private readonly IDispositivosAnalyticsService _analytics;

    public GetDispositivosResumenQueryHandler(IDispositivosAnalyticsService analytics) =>
        _analytics = analytics;

    public async Task<Result<DispositivosResumenDto>> Handle(GetDispositivosResumenQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return Result<DispositivosResumenDto>.Success(await _analytics.GetDispositivosResumenAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            return Result<DispositivosResumenDto>.Failure($"Error al obtener resumen de dispositivos: {ex.Message}");
        }
    }
}
