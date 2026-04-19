using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservas.Application.Features.Analytics.Queries;
using Barcelo.Authorization.Shared;

namespace Reservas.API.Controllers;

[Authorize]
[ApiController]
[HasPermission(Permissions.Reports.View)]
[Route("analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("ocupacion/resumen")]
    public async Task<IActionResult> GetOcupacionResumen()
    {
        var result = await _mediator.Send(new GetOcupacionResumenQuery());
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("ocupacion/por-hotel")]
    public async Task<IActionResult> GetOcupacionPorHotel()
    {
        var result = await _mediator.Send(new GetOcupacionPorHotelQuery());
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("ocupacion/historico")]
    public async Task<IActionResult> GetOcupacionHistorico()
    {
        var result = await _mediator.Send(new GetOcupacionHistoricoQuery());
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("actividades/resumen")]
    public async Task<IActionResult> GetActividadesResumen()
    {
        var result = await _mediator.Send(new GetActividadesResumenQuery());
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("actividades/mas-populares")]
    public async Task<IActionResult> GetActividadesMasPopulares([FromQuery] int top = 5)
    {
        var result = await _mediator.Send(new GetActividadesMasPopularesQuery(top));
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }
}
