using MediatR;
using Microsoft.AspNetCore.Mvc;
using Usuarios.Application.DTOs.TipoDocumento;
using Usuarios.Application.UseCases.TipoDocumento.Commands.CreateTipoDocumento;
using Usuarios.Application.UseCases.TipoDocumento.Commands.DeleteTipoDocumento;
using Usuarios.Application.UseCases.TipoDocumento.Commands.UpdateTipoDocumento;
using Usuarios.Application.UseCases.TipoDocumento.Queries.GetAllTiposDocumento;
using Usuarios.Application.UseCases.TipoDocumento.Queries.GetTipoDocumentoById;
using Barcelo.Authorization.Shared;

namespace Usuarios.API.Controllers;

[ApiController]
// [HasPermission(Permissions.Usuarios.View)]
[Route("[controller]")]
public class TipoDocumentoController : ControllerBase
{
    private readonly IMediator _mediator;

    public TipoDocumentoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllTiposDocumentoQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetTipoDocumentoByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(Permissions.Usuarios.Create)]
    public async Task<IActionResult> Create([FromBody] CreateTipoDocumentoDto dto)
    {
        var result = await _mediator.Send(new CreateTipoDocumentoCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.TipoDocumentoId }, result);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.Usuarios.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTipoDocumentoDto dto)
    {
        if (id != dto.TipoDocumentoId)
            return BadRequest("El ID no coincide");

        var result = await _mediator.Send(new UpdateTipoDocumentoCommand(dto));
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.Usuarios.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteTipoDocumentoCommand(id));
        return Ok(result);
    }
}
