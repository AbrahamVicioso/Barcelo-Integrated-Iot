using MediatR;
using Microsoft.AspNetCore.Mvc;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Features.TiposDispositivo.Commands;
using Dispositivos.Application.Features.TiposDispositivo.Queries;

namespace Dispositivos.API.Controllers;

[Route("[controller]")]
[ApiController]
public class TiposDispositivoController : ControllerBase
{
    private readonly IMediator _mediator;

    public TiposDispositivoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllTiposDispositivoQuery());
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetTipoDispositivoByIdQuery { TipoDispositivoId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTipoDispositivoDto dto)
    {
        var result = await _mediator.Send(new CreateTipoDispositivoCommand { TipoDispositivo = dto });
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTipoDispositivoCommand command)
    {
        if (id != command.TipoDispositivoId)
            return BadRequest("El ID del tipo no coincide con el ID de la solicitud.");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteTipoDispositivoCommand { TipoDispositivoId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }
}
