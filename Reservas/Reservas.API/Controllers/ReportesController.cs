using Barcelo.Authorization.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using Reservas.API.Reports;
using Reservas.Application.Interfaces;
using Reservas.Application.DTOs.Reports;

namespace Reservas.API.Controllers;

[Authorize]
[ApiController]
[HasPermission(Permissions.Reports.View)]
[Route("reportes")]
public class ReportesController : ControllerBase
{
    private readonly IReportePdfDataService _pdfData;
    private readonly ILogger<ReportesController> _logger;

    public ReportesController(IReportePdfDataService pdfData, ILogger<ReportesController> logger)
    {
        _pdfData = pdfData;
        _logger = logger;
    }

    /// GET /reportes/pdf/reservas?fechaInicio=...&fechaFin=... — PDF hoja blanca
    [HttpGet("pdf/reservas")]
    public async Task<IActionResult> PdfReservas(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        CancellationToken ct)
    {
        var items = await _pdfData.GetReservasPeriodoAsync(fechaInicio, fechaFin, ct);
        var pdf = new ReporteReservasDocument(items, fechaInicio, fechaFin).GeneratePdf();
        var suffix = fechaInicio.HasValue ? $"_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}" : "_todos";
        return File(pdf, "application/pdf", $"reservas{suffix}.pdf");
    }

    /// GET /reportes/pdf/checkin-temprano — Check-in antes de la hora programada
    [HttpGet("pdf/checkin-temprano")]
    public async Task<IActionResult> PdfCheckInTemprano([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin, CancellationToken ct)
    {
        var items = await _pdfData.GetCheckInTempranoAsync(fechaInicio, fechaFin, ct);
        var pdf = new ReportePuntualidadDocument(items,
            "Reporte Check-In Anticipado",
            "Reservas donde el check-in se realizó antes de la hora programada",
            fechaInicio, fechaFin, accentColor: "#1565c0").GeneratePdf();
        var suffix = fechaInicio.HasValue ? $"_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}" : "_todos";
        return File(pdf, "application/pdf", $"checkin_temprano{suffix}.pdf");
    }

    /// GET /reportes/pdf/checkin-tarde — Check-in después de la hora programada
    [HttpGet("pdf/checkin-tarde")]
    public async Task<IActionResult> PdfCheckInTarde([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin, CancellationToken ct)
    {
        var items = await _pdfData.GetCheckInTardeAsync(fechaInicio, fechaFin, ct);
        var pdf = new ReportePuntualidadDocument(items,
            "Reporte Check-In Tardío",
            "Reservas donde el check-in se realizó después de la hora programada",
            fechaInicio, fechaFin, accentColor: "#c62828").GeneratePdf();
        var suffix = fechaInicio.HasValue ? $"_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}" : "_todos";
        return File(pdf, "application/pdf", $"checkin_tarde{suffix}.pdf");
    }

    /// GET /reportes/pdf/checkout-temprano — Check-out antes de la hora programada
    [HttpGet("pdf/checkout-temprano")]
    public async Task<IActionResult> PdfCheckOutTemprano([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin, CancellationToken ct)
    {
        var items = await _pdfData.GetCheckOutTempranoAsync(fechaInicio, fechaFin, ct);
        var pdf = new ReportePuntualidadDocument(items,
            "Reporte Check-Out Anticipado",
            "Reservas donde el check-out se realizó antes de la hora programada",
            fechaInicio, fechaFin, accentColor: "#2e7d32").GeneratePdf();
        var suffix = fechaInicio.HasValue ? $"_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}" : "_todos";
        return File(pdf, "application/pdf", $"checkout_temprano{suffix}.pdf");
    }

    /// GET /reportes/pdf/checkout-tarde — Check-out después de la hora programada
    [HttpGet("pdf/checkout-tarde")]
    public async Task<IActionResult> PdfCheckOutTarde([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin, CancellationToken ct)
    {
        var items = await _pdfData.GetCheckOutTardeAsync(fechaInicio, fechaFin, ct);
        var pdf = new ReportePuntualidadDocument(items,
            "Reporte Check-Out Tardío",
            "Reservas donde el check-out se realizó después de la hora programada",
            fechaInicio, fechaFin, accentColor: "#e65100").GeneratePdf();
        var suffix = fechaInicio.HasValue ? $"_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}" : "_todos";
        return File(pdf, "application/pdf", $"checkout_tarde{suffix}.pdf");
    }

    /// GET /reportes/pdf/habitaciones?tipoId=&estadoId=&hotelId=
    [HttpGet("pdf/habitaciones")]
    public async Task<IActionResult> PdfHabitaciones(
        [FromQuery] int? tipoId,
        [FromQuery] int? estadoId,
        [FromQuery] int? hotelId,
        CancellationToken ct)
    {
        var items = await _pdfData.GetHabitacionesAsync(tipoId, estadoId, hotelId, ct);
        var filtros = BuildFiltrosHabitacion(tipoId, estadoId, hotelId);
        var pdf = new ReporteHabitacionesDocument(items, filtros).GeneratePdf();
        return File(pdf, "application/pdf", "habitaciones.pdf");
    }

    /// GET /reportes/pdf/actividades?fechaInicio=&fechaFin=
    [HttpGet("pdf/actividades")]
    public async Task<IActionResult> PdfActividadesResumen(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        CancellationToken ct)
    {
        var items = await _pdfData.GetActividadesResumenAsync(fechaInicio, fechaFin, ct);
        var pdf = new ReporteActividadesResumenDocument(items, fechaInicio, fechaFin).GeneratePdf();
        var suffix = fechaInicio.HasValue ? $"_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}" : "_todos";
        return File(pdf, "application/pdf", $"actividades_resumen{suffix}.pdf");
    }

    /// GET /reportes/pdf/actividades-sin-acceso?fechaInicio=&fechaFin=
    [HttpGet("pdf/actividades-sin-acceso")]
    public async Task<IActionResult> PdfActividadesSinAcceso(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        CancellationToken ct)
    {
        var items = await _pdfData.GetActividadesSinAccesoAsync(fechaInicio, fechaFin, ct);
        var pdf = new ReporteActividadesSinAccesoDocument(items, fechaInicio, fechaFin).GeneratePdf();
        var suffix = fechaInicio.HasValue ? $"_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}" : "_todos";
        return File(pdf, "application/pdf", $"actividades_sin_acceso{suffix}.pdf");
    }

    /// GET /reportes/pdf/huespedes?fechaInicio=&fechaFin=&soloVip=
    [HttpGet("pdf/huespedes")]
    public async Task<IActionResult> PdfHuespedes(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] bool? soloVip,
        CancellationToken ct)
    {
        var items = await _pdfData.GetHuespedesAsync(fechaInicio, fechaFin, soloVip, ct);
        var pdf = new ReporteHuespedesDocument(items, fechaInicio, fechaFin, soloVip).GeneratePdf();
        var suffix = soloVip == true ? "_vip" : fechaInicio.HasValue ? $"_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}" : "_todos";
        return File(pdf, "application/pdf", $"huespedes{suffix}.pdf");
    }

    /// GET /reportes/pdf/personal?departamentoId=&soloActivos=
    [HttpGet("pdf/personal")]
    public async Task<IActionResult> PdfPersonal(
        [FromQuery] int? departamentoId,
        [FromQuery] bool? soloActivos,
        CancellationToken ct)
    {
        try
        {
            var items = await _pdfData.GetPersonalAsync(departamentoId, soloActivos, ct);
            _logger.LogInformation("PdfPersonal: {Count} registros encontrados (soloActivos={SoloActivos})", items.Count, soloActivos);
            var pdf = new ReportePersonalDocument(items, soloActivos).GeneratePdf();
            return File(pdf, "application/pdf", "personal.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PdfPersonal error");
            return StatusCode(500, ex.Message);
        }
    }

    private static string BuildFiltrosHabitacion(int? tipoId, int? estadoId, int? hotelId)
    {
        var partes = new List<string>();
        if (tipoId.HasValue)   partes.Add($"Tipo: {tipoId}");
        if (estadoId.HasValue) partes.Add($"Estado: {estadoId}");
        if (hotelId.HasValue)  partes.Add($"Hotel: {hotelId}");
        return partes.Count > 0 ? string.Join(" | ", partes) : "Todas las habitaciones";
    }
}
