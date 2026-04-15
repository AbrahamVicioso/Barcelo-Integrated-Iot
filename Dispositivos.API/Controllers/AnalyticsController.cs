using Dispositivos.Application.Features.Analytics.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dispositivos.API.Controllers;

[Authorize]
[ApiController]
[Route("analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("accesos/resumen")]
    public async Task<IActionResult> GetAccesosResumen()
    {
        var result = await _mediator.Send(new GetAccesosResumenQuery());
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("dispositivos/resumen")]
    public async Task<IActionResult> GetDispositivosResumen()
    {
        var result = await _mediator.Send(new GetDispositivosResumenQuery());
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("accesos/por-tipo")]
    public async Task<IActionResult> GetAccesosPorTipo()
    {
        var result = await _mediator.Send(new GetAccesosPorTipoQuery());
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("incidentes/resumen")]
    public async Task<IActionResult> GetIncidentesResumen()
    {
        var result = await _mediator.Send(new GetIncidentesResumenQuery());
        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }
}
