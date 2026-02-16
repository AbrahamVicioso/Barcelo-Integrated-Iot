using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Features.RegistrosAcceso.Commands;
using Dispositivos.Application.Features.RegistrosAcceso.Queries;

namespace Dispositivos.API.Controllers;

[Route("[controller]")]
[ApiController]
public class RegistrosAccesoController : ControllerBase
{
    private readonly IMediator _mediator;

    public RegistrosAccesoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllRegistrosAccesoQuery());
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetRegistrosAccesoByIdQuery { RegistroId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRegistrosAccesoDto registroDto)
    {
        var result = await _mediator.Send(new CreateRegistrosAccesoCommand { Registro = registroDto });
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteRegistrosAccesoCommand { RegistroId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }
}
