using MediatR;
using Microsoft.AspNetCore.Mvc;
using Dispositivos.Application.Features.Dashboard.Queries;

namespace Dispositivos.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene las estadísticas del dashboard para dispositivos, cerraduras, credenciales y auditoría
    /// </summary>
    /// <param name="hotelId">ID del hotel para filtrar (opcional)</param>
    /// <param name="desde">Fecha inicio para filtrar auditoría (opcional)</param>
    /// <param name="hasta">Fecha fin para filtrar auditoría (opcional)</param>
    /// <returns>Estadísticas del dashboard</returns>
    [HttpGet("dispositivos-stats")]
    public async Task<IActionResult> GetDispositivosStats(
        [FromQuery] int? hotelId = null,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        var result = await _mediator.Send(new GetDashboardDispositivosStatsQuery
        {
            HotelId = hotelId,
            Desde = desde,
            Hasta = hasta
        });

        if (!result.IsSuccess)
            return result.IsNotFound
                ? NotFound(new { error = result.ErrorMessage })
                : BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Obtiene solo las estadísticas de credenciales de acceso
    /// </summary>
    [HttpGet("credentials-stats")]
    public async Task<IActionResult> GetCredentialsStats([FromQuery] int? hotelId = null)
    {
        var result = await _mediator.Send(new GetDashboardDispositivosStatsQuery { HotelId = hotelId });

        if (!result.IsSuccess)
            return result.IsNotFound
                ? NotFound(new { error = result.ErrorMessage })
                : BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data?.Credenciales);
    }

    /// <summary>
    /// Obtiene solo las estadísticas de dispositivos
    /// </summary>
    [HttpGet("devices-stats")]
    public async Task<IActionResult> GetDevicesStats([FromQuery] int? hotelId = null)
    {
        var result = await _mediator.Send(new GetDashboardDispositivosStatsQuery { HotelId = hotelId });

        if (!result.IsSuccess)
            return result.IsNotFound
                ? NotFound(new { error = result.ErrorMessage })
                : BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data?.Dispositivos);
    }

    /// <summary>
    /// Obtiene solo las estadísticas de cerraduras inteligentes
    /// </summary>
    [HttpGet("locks-stats")]
    public async Task<IActionResult> GetLocksStats([FromQuery] int? hotelId = null)
    {
        var result = await _mediator.Send(new GetDashboardDispositivosStatsQuery { HotelId = hotelId });

        if (!result.IsSuccess)
            return result.IsNotFound
                ? NotFound(new { error = result.ErrorMessage })
                : BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data?.Cerraduras);
    }

    /// <summary>
    /// Obtiene solo las estadísticas de auditoría reciente
    /// </summary>
    [HttpGet("audit-recent")]
    public async Task<IActionResult> GetAuditRecent(
        [FromQuery] int? hotelId = null,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        var result = await _mediator.Send(new GetDashboardDispositivosStatsQuery
        {
            HotelId = hotelId,
            Desde = desde,
            Hasta = hasta
        });

        if (!result.IsSuccess)
            return result.IsNotFound
                ? NotFound(new { error = result.ErrorMessage })
                : BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data?.Auditoria);
    }

    /// <summary>
    /// Obtiene la lista de alertas activas (dispositivos con batería baja u offline)
    /// </summary>
    [HttpGet("active-alerts")]
    public async Task<IActionResult> GetActiveAlerts([FromQuery] int? hotelId = null)
    {
        var result = await _mediator.Send(new GetDashboardDispositivosStatsQuery { HotelId = hotelId });

        if (!result.IsSuccess)
            return result.IsNotFound
                ? NotFound(new { error = result.ErrorMessage })
                : BadRequest(new { error = result.ErrorMessage });

        var alerts = new List<object>();

        if (result.Data?.Dispositivos?.BateriaBaja > 0)
        {
            alerts.Add(new
            {
                tipo = "bateria_baja",
                severidad = "Media",
                mensaje = $"{result.Data.Dispositivos.BateriaBaja} dispositivos con batería baja",
                cantidad = result.Data.Dispositivos.BateriaBaja,
                accion = "/admin/dispositivos?filter=bateria_baja"
            });
        }

        if (result.Data?.Dispositivos?.Offline > 0)
        {
            alerts.Add(new
            {
                tipo = "dispositivos_offline",
                severidad = "Alta",
                mensaje = $"{result.Data.Dispositivos.Offline} dispositivos offline",
                cantidad = result.Data.Dispositivos.Offline,
                accion = "/admin/dispositivos?filter=offline"
            });
        }

        if (result.Data?.Cerraduras?.Offline > 0)
        {
            alerts.Add(new
            {
                tipo = "cerraduras_offline",
                severidad = "Alta",
                mensaje = $"{result.Data.Cerraduras.Offline} cerraduras offline",
                cantidad = result.Data.Cerraduras.Offline,
                accion = "/admin/cerraduras?filter=offline"
            });
        }

        if (result.Data?.Credenciales?.PorVencer > 0)
        {
            alerts.Add(new
            {
                tipo = "credenciales_por_vencer",
                severidad = "Baja",
                mensaje = $"{result.Data.Credenciales.PorVencer} credenciales por vencer",
                cantidad = result.Data.Credenciales.PorVencer,
                accion = "/admin/credenciales?filter=por_vencer"
            });
        }

        return Ok(alerts);
    }
}
