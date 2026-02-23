using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Features.Dispositivos.Commands;
using Dispositivos.Application.Features.Dispositivos.Queries;

namespace Dispositivos.API.Controllers;

[Route("[controller]")]
[ApiController]
public class DispositivoController : ControllerBase
{
    private readonly IMediator _mediator;

    public DispositivoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllDispositivosQuery());
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetDispositivoByIdQuery { DispositivoId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpGet("hotel/{hotelId}")]
    public async Task<IActionResult> GetByHotelId(int hotelId)
    {
        var result = await _mediator.Send(new GetDispositivosByHotelIdQuery { HotelId = hotelId });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }
      

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDispositivoDto dispositivoDto)
    {
        var result = await _mediator.Send(new CreateDispositivoCommand { Dispositivo = dispositivoDto });
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDispositivoDto dispositivoDto)
    {
        if (id != dispositivoDto.DispositivoId)
        {
            return BadRequest("El ID del dispositivo no coincide con el ID de la solicitud.");
        }

        var result = await _mediator.Send(new UpdateDispositivoCommand { DispositivoId = id, Dispositivo = dispositivoDto });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteDispositivoCommand { DispositivoId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }
}
