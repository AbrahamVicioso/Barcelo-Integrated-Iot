using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Features.CerradurasInteligente.Commands;
using Dispositivos.Application.Features.CerradurasInteligente.Queries;

namespace Dispositivos.API.Controllers;

[Route("[controller]")]
[ApiController]
public class CerradurasInteligenteController : ControllerBase
{
    private readonly IMediator _mediator;

    public CerradurasInteligenteController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllCerradurasInteligenteQuery());
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetCerradurasInteligenteByIdQuery { CerraduraId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCerradurasInteligenteDto cerraduraDto)
    {
        var result = await _mediator.Send(new CreateCerradurasInteligenteCommand { Cerradura = cerraduraDto });
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCerradurasInteligenteDto cerraduraDto)
    {
        if (id != cerraduraDto.CerraduraId)
        {
            return BadRequest("El ID de la cerradura no coincide con el ID de la solicitud.");
        }

        var result = await _mediator.Send(new UpdateCerradurasInteligenteCommand { Cerradura = cerraduraDto });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteCerradurasInteligenteCommand { CerraduraId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }
}
