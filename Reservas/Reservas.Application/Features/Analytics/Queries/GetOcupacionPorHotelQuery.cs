using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs.Analytics;
using Reservas.Application.Interfaces;

namespace Reservas.Application.Features.Analytics.Queries;

public record GetOcupacionPorHotelQuery : IRequest<Result<List<OcupacionPorHotelDto>>>;

public class GetOcupacionPorHotelQueryHandler : IRequestHandler<GetOcupacionPorHotelQuery, Result<List<OcupacionPorHotelDto>>>
{
    private readonly IReservasAnalyticsService _analytics;

    public GetOcupacionPorHotelQueryHandler(IReservasAnalyticsService analytics) =>
        _analytics = analytics;

    public async Task<Result<List<OcupacionPorHotelDto>>> Handle(GetOcupacionPorHotelQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return Result<List<OcupacionPorHotelDto>>.Success(await _analytics.GetOcupacionPorHotelAsync(cancellationToken));
        }
        catch (Exception ex)
        {
            return Result<List<OcupacionPorHotelDto>>.Failure($"Error al obtener ocupación por hotel: {ex.Message}");
        }
    }
}
