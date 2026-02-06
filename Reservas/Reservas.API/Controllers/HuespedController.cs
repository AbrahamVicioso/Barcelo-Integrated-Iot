using MediatR;
using Microsoft.AspNetCore.Mvc;
using Reservas.Application.DTOs;
using Reservas.Application.Features.Huespedes.Queries;

namespace Reservas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HuespedController : ControllerBase
{
    private readonly IMediator _mediator;

    public HuespedController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("user/{usuarioId}")]
    public async Task<ActionResult<HuespedeDto?>> GetHuespedByUsuarioId(string usuarioId)
    {
        var query = new GetHuespedByUsuarioIdQuery(usuarioId);
        var huesped = await _mediator.Send(query);

        if (huesped == null)
        {
            return NotFound(new { message = $"Huésped no encontrado para el usuarioId: {usuarioId}" });
        }

        return Ok(huesped);
    }
}
