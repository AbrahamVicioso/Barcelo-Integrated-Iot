using MediatR;
using Microsoft.AspNetCore.Mvc;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Features.TiposDispositivo.Commands;
using Dispositivos.Application.Features.TiposDispositivo.Queries;
using Barcelo.Authorization.Shared;

namespace Dispositivos.API.Controllers;

[Route("[controller]")]
[ApiController]
[HasPermission(Permissions.Dispositivos.View)]
public class TiposDispositivoController : ControllerBase
{
    private readonly IMediator _mediator;

    public TiposDispositivoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        var result = await _mediator.Send(new GetAllTiposDispositivoQuery { Page = pagination.Page, PageSize = pagination.PageSize });
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetTipoDispositivoByIdQuery { TipoDispositivoId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpPost]
    [HasPermission(Permissions.Dispositivos.Create)]
    public async Task<IActionResult> Create([FromBody] CreateTipoDispositivoDto dto)
    {
        var result = await _mediator.Send(new CreateTipoDispositivoCommand { TipoDispositivo = dto });
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.Dispositivos.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTipoDispositivoCommand command)
    {
        if (id != command.TipoDispositivoId)
            return BadRequest("El ID del tipo no coincide con el ID de la solicitud.");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.Dispositivos.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteTipoDispositivoCommand { TipoDispositivoId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }
}
