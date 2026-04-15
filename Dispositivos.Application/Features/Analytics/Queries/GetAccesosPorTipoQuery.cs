using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs.Analytics;
using Dispositivos.Application.Interfaces;
using MediatR;

namespace Dispositivos.Application.Features.Analytics.Queries;

public record GetAccesosPorTipoQuery : IRequest<Result<List<AccesoPorTipoDto>>>;

public class GetAccesosPorTipoQueryHandler : IRequestHandler<GetAccesosPorTipoQuery, Result<List<AccesoPorTipoDto>>>
{
    private readonly IDispositivosAnalyticsService _analytics;

    public GetAccesosPorTipoQueryHandler(IDispositivosAnalyticsService analytics) =>
        _analytics = analytics;

    public async Task<Result<List<AccesoPorTipoDto>>> Handle(GetAccesosPorTipoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return Result<List<AccesoPorTipoDto>>.Success(await _analytics.GetAccesosPorTipoAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            return Result<List<AccesoPorTipoDto>>.Failure($"Error al obtener accesos por tipo: {ex.Message}");
        }
    }
}
