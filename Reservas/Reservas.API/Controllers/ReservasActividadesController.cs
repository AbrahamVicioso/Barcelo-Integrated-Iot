using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservas.Application.Features.Historial.Queries;
using Reservas.Application.Features.ReservasActividades.Commands;
using Reservas.Application.Features.ReservasActividades.Queries;
using System.Security.Claims;
using Barcelo.Authorization.Shared;

namespace Reservas.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ReservasActividadesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservasActividadesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyReservas()
    {
        var usuarioId = User.FindFirstValue("nameid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (usuarioId == null)
            return Unauthorized();

        var result = await _mediator.Send(new GetReservasActividadByUsuarioIdQuery(usuarioId));
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [Authorize]
    [HttpPost("me")]
    public async Task<IActionResult> CreateMe([FromBody] CreateReservaActividadMeCommand command)
    {
        var usuarioId = User.FindFirstValue("nameid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (usuarioId == null)
            return Unauthorized();

        command.UsuarioId = usuarioId;
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(result.ErrorMessage) : BadRequest(result.ErrorMessage);
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.ReservaActividadId }, result.Data);
    }

    [Authorize]
    [HttpPut("me/{id}")]
    public async Task<IActionResult> UpdateMe(int id, [FromBody] UpdateReservaActividadMeCommand command)
    {
        var usuarioId = User.FindFirstValue("nameid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (usuarioId == null)
            return Unauthorized();

        if (id != command.ReservaActividadId)
            return BadRequest("El ID de la ruta no coincide con el ID del comando");

        command.UsuarioId = usuarioId;
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(result.ErrorMessage) : BadRequest(result.ErrorMessage);
        return Ok(result.Data);
    }

    [Authorize]
    [HttpDelete("me/{id}")]
    public async Task<IActionResult> DeleteMe(int id)
    {
        var usuarioId = User.FindFirstValue("nameid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (usuarioId == null)
            return Unauthorized();

        var result = await _mediator.Send(new DeleteReservaActividadMeCommand
        {
            UsuarioId = usuarioId,
            ReservaActividadId = id
        });
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(result.ErrorMessage) : BadRequest(result.ErrorMessage);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me/historial")]
    public async Task<IActionResult> GetMyHistorial(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var usuarioId = User.FindFirstValue("nameid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (usuarioId == null)
            return Unauthorized();

        var result = await _mediator.Send(new GetHistorialActividadesMeQuery
        {
            UsuarioId = usuarioId,
            Page = page,
            PageSize = pageSize
        });

        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(result.ErrorMessage) : BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllReservasActividadesQuery());
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetReservaActividadByIdQuery { ReservaActividadId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpPost]
    [HasPermission(Permissions.Reservas.Create)]
    public async Task<IActionResult> Create([FromBody] CreateReservaActividadCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data?.ReservaActividadId }, result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.Reservas.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReservaActividadCommand command)
    {
        if (id != command.ReservaActividadId)
            return BadRequest("El ID de la ruta no coincide con el ID del comando");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.Reservas.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteReservaActividadCommand { ReservaActividadId = id });
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}/estado")]
    [HasPermission(Permissions.Reservas.Edit)]
    public async Task<IActionResult> UpdateEstado(int id, [FromQuery] int estadoId)
    {
        var result = await _mediator.Send(new UpdateReservaActividadEstadoCommand { ReservaActividadId = id, EstadoReservaActividadId = estadoId });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

}
