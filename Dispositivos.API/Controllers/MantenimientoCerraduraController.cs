using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Features.MantenimientoCerradura.Commands;
using Dispositivos.Application.Features.MantenimientoCerradura.Queries;

namespace Dispositivos.API.Controllers;

[Route("[controller]")]
[ApiController]
public class MantenimientoCerraduraController : ControllerBase
{
    private readonly IMediator _mediator;

    public MantenimientoCerraduraController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllMantenimientoCerraduraQuery());
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetMantenimientoCerraduraByIdQuery { MantenimientoId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMantenimientoCerraduraDto mantenimientoDto)
    {
        var result = await _mediator.Send(new CreateMantenimientoCerraduraCommand { Mantenimiento = mantenimientoDto });
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMantenimientoCerraduraDto mantenimientoDto)
    {
        if (id != mantenimientoDto.MantenimientoId)
        {
            return BadRequest("El ID del mantenimiento no coincide con el ID de la solicitud.");
        }

        var result = await _mediator.Send(new UpdateMantenimientoCerraduraCommand { Mantenimiento = mantenimientoDto });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteMantenimientoCerraduraCommand { MantenimientoId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }
}
