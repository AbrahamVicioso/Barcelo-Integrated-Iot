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
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetCredencialesAccesoByIdQuery { CredencialId = id });
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCredencialesAccesoDto credencialDto)
    {
        var result = await _mediator.Send(new CreateCredencialesAccesoCommand { Credencial = credencialDto });
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCredencialesAccesoDto credencialDto)
    {
        if (id != credencialDto.CredencialId)
            return BadRequest(new { error = "El ID de la credencial no coincide con el ID de la solicitud." });

        var result = await _mediator.Send(new UpdateCredencialesAccesoCommand { Credencial = credencialDto });
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteCredencialesAccesoCommand { CredencialId = id });
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }
}
