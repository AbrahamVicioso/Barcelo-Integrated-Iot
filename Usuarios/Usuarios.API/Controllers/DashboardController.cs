using MediatR;
using Microsoft.AspNetCore.Mvc;
using Usuarios.Application.UseCases.Dashboard.Queries.GetHuespedesStats;
using Usuarios.Application.UseCases.Dashboard.Queries.GetPersonalStats;

namespace Usuarios.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene las estadísticas de huéspedes
    /// </summary>
    /// <param name="hotelId">ID del hotel para filtrar (opcional)</param>
    /// <returns>Estadísticas de huéspedes</returns>
    [HttpGet("guests-stats")]
    public async Task<IActionResult> GetGuestsStats([FromQuery] int? hotelId = null)
    {
        var result = await _mediator.Send(new GetHuespedesStatsQuery(hotelId));
        return Ok(result);
    }

    /// <summary>
    /// Obtiene las estadísticas de personal
    /// </summary>
    /// <param name="hotelId">ID del hotel para filtrar (opcional)</param>
    /// <returns>Estadísticas de personal</returns>
    [HttpGet("staff-stats")]
    public async Task<IActionResult> GetStaffStats([FromQuery] int? hotelId = null)
    {
        var result = await _mediator.Send(new GetPersonalStatsQuery(hotelId));
        return Ok(result);
    }
}
