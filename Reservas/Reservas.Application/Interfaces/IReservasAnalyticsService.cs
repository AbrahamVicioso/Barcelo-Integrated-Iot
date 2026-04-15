using Reservas.Application.DTOs.Analytics;

namespace Reservas.Application.Interfaces;

public interface IReservasAnalyticsService
{
    Task<OcupacionResumenDto> GetOcupacionResumenAsync(CancellationToken ct = default);
    Task<List<OcupacionPorHotelDto>> GetOcupacionPorHotelAsync(CancellationToken ct = default);
    Task<List<OcupacionHistoricoDto>> GetOcupacionHistoricoAsync(CancellationToken ct = default);
    Task<ActividadesResumenDto> GetActividadesResumenAsync(CancellationToken ct = default);
    Task<List<ActividadPopularDto>> GetActividadesMasPopularesAsync(int top, CancellationToken ct = default);
}
