using MediatR;
using Microsoft.AspNetCore.Mvc;
using Reservas.Application.Features.Dashboard.Queries;
using Barcelo.Authorization.Shared;

namespace Reservas.API.Controllers;

[Route("[controller]")]
[ApiController]
[HasPermission(Permissions.Reports.View)]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene las estadísticas del dashboard para hoteles, habitaciones, reservas y actividades
    /// </summary>
    /// <param name="hotelId">ID del hotel para filtrar (opcional)</param>
    /// <param name="desde">Fecha inicio para filtrar (opcional)</param>
    /// <param name="hasta">Fecha fin para filtrar (opcional)</param>
    /// <returns>Estadísticas del dashboard</returns>
    [HttpGet("reservas-stats")]
    public async Task<IActionResult> GetReservasStats(
        [FromQuery] int? hotelId = null,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        var result = await _mediator.Send(new GetDashboardReservasStatsQuery
        {
            HotelId = hotelId,
            Desde = desde,
            Hasta = hasta
        });

        return result.IsSuccess
            ? Ok(result.Data)
            : BadRequest(new { error = result.ErrorMessage });
    }

    /// <summary>
    /// Obtiene solo las estadísticas de hoteles
    /// </summary>
    [HttpGet("hotels-stats")]
    public async Task<IActionResult> GetHotelsStats([FromQuery] int? hotelId = null)
    {
        var result = await _mediator.Send(new GetDashboardReservasStatsQuery { HotelId = hotelId });
        return result.IsSuccess
            ? Ok(result.Data?.Hoteles)
            : BadRequest(new { error = result.ErrorMessage });
    }

    /// <summary>
    /// Obtiene solo las estadísticas de habitaciones
    /// </summary>
    [HttpGet("rooms-stats")]
    public async Task<IActionResult> GetRoomsStats([FromQuery] int? hotelId = null)
    {
        var result = await _mediator.Send(new GetDashboardReservasStatsQuery { HotelId = hotelId });
        return result.IsSuccess
            ? Ok(result.Data?.Habitaciones)
            : BadRequest(new { error = result.ErrorMessage });
    }

    /// <summary>
    /// Obtiene solo las estadísticas de reservas
    /// </summary>
    [HttpGet("reservations-stats")]
    public async Task<IActionResult> GetReservationsStats(
        [FromQuery] int? hotelId = null,
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        var result = await _mediator.Send(new GetDashboardReservasStatsQuery
        {
            HotelId = hotelId,
            Desde = desde,
            Hasta = hasta
        });
        return result.IsSuccess
            ? Ok(result.Data?.Reservas)
            : BadRequest(new { error = result.ErrorMessage });
    }

    /// <summary>
    /// Obtiene solo las estadísticas de actividades recreativas
    /// </summary>
    [HttpGet("activities-stats")]
    public async Task<IActionResult> GetActivitiesStats([FromQuery] int? hotelId = null)
    {
        var result = await _mediator.Send(new GetDashboardReservasStatsQuery { HotelId = hotelId });
        return result.IsSuccess
            ? Ok(result.Data?.Actividades)
            : BadRequest(new { error = result.ErrorMessage });
    }

    /// <summary>
    /// Obtiene la lista de check-ins programados para hoy
    /// </summary>
    [HttpGet("today-checkins")]
    public async Task<IActionResult> GetTodayCheckIns([FromQuery] int? hotelId = null)
    {
        var result = await _mediator.Send(new GetDashboardReservasStatsQuery { HotelId = hotelId });
        return result.IsSuccess
            ? Ok(result.Data?.Reservas?.CheckInsDeHoy)
            : BadRequest(new { error = result.ErrorMessage });
    }

    /// <summary>
    /// Obtiene la lista de check-outs programados para hoy
    /// </summary>
    [HttpGet("today-checkouts")]
    public async Task<IActionResult> GetTodayCheckOuts([FromQuery] int? hotelId = null)
    {
        var result = await _mediator.Send(new GetDashboardReservasStatsQuery { HotelId = hotelId });
        return result.IsSuccess
            ? Ok(result.Data?.Reservas?.CheckOutsDeHoy)
            : BadRequest(new { error = result.ErrorMessage });
    }
}
