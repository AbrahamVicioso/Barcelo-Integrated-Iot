using MediatR;
using Microsoft.AspNetCore.Mvc;
using Reservas.Application.Features.ActividadesRecreativas.Commands;
using Reservas.Application.Features.ActividadesRecreativas.Queries;

namespace Reservas.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ActividadesRecreativasController : ControllerBase
{
    private readonly IMediator _mediator;

    public ActividadesRecreativasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllActividadesQuery());
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetActividadByIdQuery { ActividadId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateActividadRecreativaCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data?.ActividadId }, result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateActividadRecreativaCommand command)
    {
        if (id != command.ActividadId)
            return BadRequest("El ID de la ruta no coincide con el ID del comando");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteActividadRecreativaCommand { ActividadId = id });
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage);
    }
}
