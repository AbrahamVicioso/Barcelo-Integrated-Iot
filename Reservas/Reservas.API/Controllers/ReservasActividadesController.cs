using MediatR;
using Microsoft.AspNetCore.Mvc;
using Reservas.Application.Features.ReservasActividades.Commands;
using Reservas.Application.Features.ReservasActividades.Queries;

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
    public async Task<IActionResult> Create([FromBody] CreateReservaActividadCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data?.ReservaActividadId }, result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReservaActividadCommand command)
    {
        if (id != command.ReservaActividadId)
            return BadRequest("El ID de la ruta no coincide con el ID del comando");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteReservaActividadCommand { ReservaActividadId = id });
        return result.IsSuccess ? NoContent() : BadRequest(result.ErrorMessage);
    }
}
