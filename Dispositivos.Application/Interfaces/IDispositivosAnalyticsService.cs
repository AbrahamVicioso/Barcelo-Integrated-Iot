using Dispositivos.Application.DTOs.Analytics;

namespace Dispositivos.Application.Interfaces;

public interface IDispositivosAnalyticsService
{
    Task<AccesosResumenDto> GetAccesosResumenAsync(CancellationToken ct = default);
    Task<DispositivosResumenDto> GetDispositivosResumenAsync(CancellationToken ct = default);
    Task<List<AccesoPorTipoDto>> GetAccesosPorTipoAsync(CancellationToken ct = default);
    Task<IncidentesResumenDto> GetIncidentesResumenAsync(CancellationToken ct = default);
}
