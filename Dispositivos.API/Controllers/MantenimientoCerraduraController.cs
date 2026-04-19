using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Features.MantenimientoCerradura.Commands;
using Dispositivos.Application.Features.MantenimientoCerradura.Queries;
using Barcelo.Authorization.Shared;

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
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        var result = await _mediator.Send(new GetAllMantenimientoCerraduraQuery { Page = pagination.Page, PageSize = pagination.PageSize });
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetMantenimientoCerraduraByIdQuery { MantenimientoId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }

    [HttpPost]
    [HasPermission(Permissions.Mantenimientos.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMantenimientoCerraduraDto mantenimientoDto)
    {
        var result = await _mediator.Send(new CreateMantenimientoCerraduraCommand { Mantenimiento = mantenimientoDto });
        return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data) : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.Mantenimientos.Edit)]
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
    [HasPermission(Permissions.Mantenimientos.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteMantenimientoCerraduraCommand { MantenimientoId = id });
        return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
    }
}
