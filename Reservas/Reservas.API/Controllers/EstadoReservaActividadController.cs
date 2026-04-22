using MediatR;
using Microsoft.AspNetCore.Mvc;
using Reservas.Application.Features.EstadosReservaActividad.Commands;
using Reservas.Application.Features.EstadosReservaActividad.Queries;
using Barcelo.Authorization.Shared;

namespace Reservas.API.Controllers;

[ApiController]
[HasPermission(Permissions.Reservas.View)]
[Route("[controller]")]
public class EstadoReservaActividadController : ControllerBase
{
    private readonly IMediator _mediator;

    public EstadoReservaActividadController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllEstadosReservaActividadQuery());
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetEstadoReservaActividadByIdQuery { EstadoReservaActividadId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpPost]
    [HasPermission(Permissions.Reservas.Create)]
    public async Task<IActionResult> Create([FromBody] CreateEstadoReservaActividadCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data?.EstadoReservaActividadId }, result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.Reservas.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEstadoReservaActividadCommand command)
    {
        if (id != command.EstadoReservaActividadId)
            return BadRequest("El ID de la ruta no coincide con el ID del comando");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.Reservas.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteEstadoReservaActividadCommand { EstadoReservaActividadId = id });
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage);
    }
}