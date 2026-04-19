using MediatR;
using Microsoft.AspNetCore.Mvc;
using Reservas.Application.DTOs;
using Reservas.Application.Features.EstadosHabitacion.Commands;
using Reservas.Application.Features.EstadosHabitacion.Queries;
using Barcelo.Authorization.Shared;

namespace Reservas.API.Controllers;

[Route("[controller]")]
[ApiController]
[HasPermission(Permissions.Habitaciones.View)]
public class EstadosHabitacionController : ControllerBase
{
    private readonly IMediator _mediator;

    public EstadosHabitacionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllEstadosHabitacionQuery());
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetEstadoHabitacionByIdQuery { EstadoHabitacionId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpPost]
    [HasPermission(Permissions.Habitaciones.Create)]
    public async Task<IActionResult> Create([FromBody] CreateEstadoHabitacionDto dto)
    {
        var result = await _mediator.Send(new CreateEstadoHabitacionCommand { EstadoHabitacion = dto });
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.Habitaciones.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEstadoHabitacionCommand command)
    {
        if (id != command.EstadoHabitacionId)
            return BadRequest("El ID del estado no coincide con el ID de la solicitud.");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.Habitaciones.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteEstadoHabitacionCommand { EstadoHabitacionId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }
}
