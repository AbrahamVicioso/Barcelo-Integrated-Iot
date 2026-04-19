using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Features.CerradurasInteligente.Commands;
using Dispositivos.Application.Features.CerradurasInteligente.Queries;
using Barcelo.Authorization.Shared;

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
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        var result = await _mediator.Send(new GetAllCerradurasInteligenteQuery { Page = pagination.Page, PageSize = pagination.PageSize });
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetCerradurasInteligenteByIdQuery { CerraduraId = id });
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPost]
    [HasPermission(Permissions.Cerraduras.Create)]
    public async Task<IActionResult> Create([FromBody] CreateCerradurasInteligenteDto cerraduraDto)
    {
        var result = await _mediator.Send(new CreateCerradurasInteligenteCommand { Cerradura = cerraduraDto });
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.Cerraduras.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCerradurasInteligenteDto cerraduraDto)
    {
        if (id != cerraduraDto.CerraduraId)
            return BadRequest(new { error = "El ID de la cerradura no coincide con el ID de la solicitud." });

        var result = await _mediator.Send(new UpdateCerradurasInteligenteCommand { Cerradura = cerraduraDto });
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.Cerraduras.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteCerradurasInteligenteCommand { CerraduraId = id });
        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }
}
