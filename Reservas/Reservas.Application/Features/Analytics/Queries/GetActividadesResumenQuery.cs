using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs.Analytics;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Analytics.Queries;

public record GetActividadesResumenQuery : IRequest<Result<ActividadesResumenDto>>;

public class GetActividadesResumenQueryHandler : IRequestHandler<GetActividadesResumenQuery, Result<ActividadesResumenDto>>
{
    private readonly IReservasAnalyticsService _analytics;

    public GetActividadesResumenQueryHandler(IReservasAnalyticsService analytics) =>
        _analytics = analytics;

    public async Task<Result<ActividadesResumenDto>> Handle(GetActividadesResumenQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return Result<ActividadesResumenDto>.Success(await _analytics.GetActividadesResumenAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            return Result<ActividadesResumenDto>.Failure($"Error al obtener resumen de actividades: {ex.Message}");
        }
    }
}
