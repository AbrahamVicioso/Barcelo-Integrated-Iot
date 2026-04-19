using MediatR;
using Microsoft.AspNetCore.Mvc;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Features.EstadosDispositivo.Commands;
using Dispositivos.Application.Features.EstadosDispositivo.Queries;
using Barcelo.Authorization.Shared;

namespace Dispositivos.API.Controllers;

[Route("[controller]")]
[ApiController]
public class EstadosDispositivoController : ControllerBase
{
    private readonly IMediator _mediator;

    public EstadosDispositivoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        var result = await _mediator.Send(new GetAllEstadosDispositivoQuery { Page = pagination.Page, PageSize = pagination.PageSize });
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetEstadoDispositivoByIdQuery { EstadoDispositivoId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpPost]
    [HasPermission(Permissions.Dispositivos.Create)]
    public async Task<IActionResult> Create([FromBody] CreateEstadoDispositivoDto dto)
    {
        var result = await _mediator.Send(new CreateEstadoDispositivoCommand { EstadoDispositivo = dto });
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.Dispositivos.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEstadoDispositivoCommand command)
    {
        if (id != command.EstadoDispositivoId)
            return BadRequest("El ID del estado no coincide con el ID de la solicitud.");

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteEstadoDispositivoCommand { EstadoDispositivoId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }
}
