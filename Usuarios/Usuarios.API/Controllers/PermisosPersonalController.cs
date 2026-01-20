using MediatR;
using Microsoft.AspNetCore.Mvc;
using Usuarios.Application.DTOs.PermisosPersonal;
using Usuarios.Application.UseCases.PermisosPersonal.Commands.CreatePermiso;
using Usuarios.Application.UseCases.PermisosPersonal.Commands.DeletePermiso;
using Usuarios.Application.UseCases.PermisosPersonal.Commands.UpdatePermiso;
using Usuarios.Application.UseCases.PermisosPersonal.Queries.GetAllPermisos;
using Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisoById;
using Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisosActivos;
using Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisosByPersonal;

namespace Usuarios.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermisosPersonalController : ControllerBase
{
    private readonly IMediator _mediator;

    public PermisosPersonalController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllPermisosQuery());
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetPermisoByIdQuery(id));
        if (!result.IsSuccess)
            return NotFound(result);
        return Ok(result);
    }

    [HttpGet("personal/{personalId}")]
    public async Task<IActionResult> GetByPersonal(int personalId)
    {
        var result = await _mediator.Send(new GetPermisosByPersonalQuery(personalId));
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("personal/{personalId}/activos")]
    public async Task<IActionResult> GetActivos(int personalId)
    {
        var result = await _mediator.Send(new GetPermisosActivosQuery(personalId));
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePermisosPersonalDto dto)
    {
        var result = await _mediator.Send(new CreatePermisoCommand(dto));
        if (!result.IsSuccess)
            return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.PermisoId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePermisosPersonalDto dto)
    {
        if (id != dto.PermisoId)
            return BadRequest("El ID no coincide");

        var result = await _mediator.Send(new UpdatePermisoCommand(dto));
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeletePermisoCommand(id));
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }
}
