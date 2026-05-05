using MediatR;
using Microsoft.AspNetCore.Mvc;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Features.CredencialesAcceso.Commands;
using Dispositivos.Application.Features.CredencialesAcceso.Queries;
using Barcelo.Authorization.Shared;

namespace Dispositivos.API.Controllers;

[Route("[controller]")]
[ApiController]
[HasPermission(Permissions.Credenciales.View)]
public class CredencialesAccesoController : ControllerBase
{
    private readonly IMediator _mediator;

    public CredencialesAccesoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.Credenciales.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        var result = await _mediator.Send(new GetAllCredencialesAccesoQuery { Page = pagination.Page, PageSize = pagination.PageSize });
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("huesped/{huespedId}")]
    [HasPermission(Permissions.Credenciales.View)]
    public async Task<IActionResult> GetByHuespedId(int huespedId, [FromQuery] PaginationParams pagination)
    {
        var result = await _mediator.Send(new GetCredencialesByHuespedIdQuery { HuespedId = huespedId, Page = pagination.Page, PageSize = pagination.PageSize });
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.Credenciales.View)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetCredencialesAccesoByIdQuery { CredencialId = id });
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost]
    [HasPermission(Permissions.Credenciales.Create)]
    public async Task<IActionResult> Create([FromBody] CreateCredencialesAccesoDto credencialDto)
    {
        var result = await _mediator.Send(new CreateCredencialesAccesoCommand { Credencial = credencialDto });
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.Credenciales.Edit)]
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
    [HasPermission(Permissions.Credenciales.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteCredencialesAccesoCommand { CredencialId = id });
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

}
