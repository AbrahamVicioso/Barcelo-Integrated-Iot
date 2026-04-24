 using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Reservas.Application.Features.Reservas.Commands;
using Reservas.Application.Features.Reservas.Queries;
using System.IdentityModel.Tokens.Jwt;
using Barcelo.Authorization.Shared;

namespace Reservas.API.Controllers;

[ApiController]
[Route("")]
public class ReservasController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    //[Authorize]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? estadoReservaId,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] string? nombreHuesped)
    {
        var result = await _mediator.Send(new GetAllReservasQuery
        {
            EstadoReservaId = estadoReservaId,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            NombreHuesped = nombreHuesped
        });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyReservations()
    {
        // Get the userId from the JWT claim
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("No se pudo identificar al usuario");
        }

        var result = await _mediator.Send(new GetReservasByUserIdFromApiQuery { UserId = userId });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("me/reserva/{reservaId}/credenciales")]
    [Authorize]
    public async Task<IActionResult> GetMyCredenciales(int reservaId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("No se pudo identificar al usuario");
        }

        var result = await _mediator.Send(new GetReservaCredencialesQuery 
        { 
            ReservaId = reservaId, 
            UserId = userId 
        });

        if (!result.IsSuccess)
            return result.IsNotFound
                ? NotFound(new { error = result.ErrorMessage })
                : BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetReservaByIdQuery { ReservaId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpGet("huesped/{huespedId}")]
    public async Task<IActionResult> GetByHuespedId(int huespedId)
    {
        var result = await _mediator.Send(new GetReservasByHuespedIdQuery { HuespedId = huespedId });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetByUserId(string userId)
    {
        var result = await _mediator.Send(new GetReservasByUserIdQuery { UserId = userId });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [Authorize]
    [HasPermission(Permissions.Reservas.Create)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservaCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data?.ReservaId }, result.Data) : BadRequest(result.ErrorMessage);
    }

    [HasPermission(Permissions.Reservas.Edit)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReservaCommand command)
    {
        if (id != command.ReservaId)
            return BadRequest("El ID de la ruta no coincide con el ID del comando");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.Reservas.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteReservaCommand { ReservaId = id });
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage);
    }

    [HttpPost("{id}/unlock-door")]
    [Authorize]
    public async Task<IActionResult> UnlockDoor(int id, [FromQuery] string? pin = null)
    {
        var result = await _mediator.Send(new UnlockDoorCommand { ReservaId = id, Pin = pin });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPost("{id}/checkin")]
    [HasPermission(Permissions.Reservas.Edit)]
    public async Task<IActionResult> PerformCheckIn(int id)
    {
        var result = await _mediator.Send(new PerformCheckInCommand { ReservaId = id });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPost("{id}/checkout")]
    [HasPermission(Permissions.Reservas.Edit)]
    public async Task<IActionResult> PerformCheckOut(int id)
    {
        var result = await _mediator.Send(new PerformCheckOutCommand { ReservaId = id });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("estados")]
    [HasPermission(Permissions.Reservas.View)]
    public async Task<IActionResult> GetEstados()
    {
        var result = await _mediator.Send(new GetAllEstadosReservaQuery());
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }
}
