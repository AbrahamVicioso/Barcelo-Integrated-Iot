using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Features.CredencialesAcceso.Commands;
using Dispositivos.Application.Features.CredencialesAcceso.Queries;

namespace Dispositivos.API.Controllers;

[Route("[controller]")]
[ApiController]
public class CredencialesAccesoController : ControllerBase
{
    private readonly IMediator _mediator;

    public CredencialesAccesoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllCredencialesAccesoQuery());
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetCredencialesAccesoByIdQuery { CredencialId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCredencialesAccesoDto credencialDto)
    {
        var result = await _mediator.Send(new CreateCredencialesAccesoCommand { Credencial = credencialDto });
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCredencialesAccesoDto credencialDto)
    {
        if (id != credencialDto.CredencialId)
        {
            return BadRequest("El ID de la credencial no coincide con el ID de la solicitud.");
        }

        var result = await _mediator.Send(new UpdateCredencialesAccesoCommand { Credencial = credencialDto });
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteCredencialesAccesoCommand { CredencialId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }
}
