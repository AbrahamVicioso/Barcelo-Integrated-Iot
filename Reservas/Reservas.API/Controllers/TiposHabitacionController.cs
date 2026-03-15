using MediatR;
using Microsoft.AspNetCore.Mvc;
using Reservas.Application.DTOs;
using Reservas.Application.Features.TiposHabitacion.Commands;
using Reservas.Application.Features.TiposHabitacion.Queries;

namespace Reservas.API.Controllers;

[Route("[controller]")]
[ApiController]
public class TiposHabitacionController : ControllerBase
{
    private readonly IMediator _mediator;

    public TiposHabitacionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllTiposHabitacionQuery());
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetTipoHabitacionByIdQuery { TipoHabitacionId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTipoHabitacionDto dto)
    {
        var result = await _mediator.Send(new CreateTipoHabitacionCommand { TipoHabitacion = dto });
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTipoHabitacionCommand command)
    {
        if (id != command.TipoHabitacionId)
            return BadRequest("El ID del tipo de habitacion no coincide con el ID de la solicitud.");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteTipoHabitacionCommand { TipoHabitacionId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }
}
