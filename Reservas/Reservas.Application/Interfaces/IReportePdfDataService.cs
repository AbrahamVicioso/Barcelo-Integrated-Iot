using Reservas.Application.DTOs.Reports;

namespace Reservas.Application.Interfaces;

public interface IReportePdfDataService
{
    Task<List<ReporteReservaItemDto>> GetReservasPeriodoAsync(DateTime? fechaInicio, DateTime? fechaFin, CancellationToken ct = default);
    Task<List<ReportePuntualidadItemDto>> GetCheckInTempranoAsync(DateTime? fechaInicio, DateTime? fechaFin, CancellationToken ct = default);
    Task<List<ReportePuntualidadItemDto>> GetCheckInTardeAsync(DateTime? fechaInicio, DateTime? fechaFin, CancellationToken ct = default);
    Task<List<ReportePuntualidadItemDto>> GetCheckOutTempranoAsync(DateTime? fechaInicio, DateTime? fechaFin, CancellationToken ct = default);
    Task<List<ReportePuntualidadItemDto>> GetCheckOutTardeAsync(DateTime? fechaInicio, DateTime? fechaFin, CancellationToken ct = default);

    Task<List<ReporteHabitacionItemDto>> GetHabitacionesAsync(int? tipoId, int? estadoId, int? hotelId, CancellationToken ct = default);
    Task<List<ReporteActividadResumenItemDto>> GetActividadesResumenAsync(DateTime? fechaInicio, DateTime? fechaFin, CancellationToken ct = default);
    Task<List<ReporteActividadSinAccesoItemDto>> GetActividadesSinAccesoAsync(DateTime? fechaInicio, DateTime? fechaFin, CancellationToken ct = default);
    Task<List<ReporteHuespedItemDto>> GetHuespedesAsync(DateTime? fechaInicio, DateTime? fechaFin, bool? soloVip, CancellationToken ct = default);
    Task<List<ReportePersonalItemDto>> GetPersonalAsync(int? departamentoId, bool? soloActivos, CancellationToken ct = default);
}
