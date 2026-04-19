using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Usuarios.Application.DTOs.PermisosPersonal;
using Usuarios.Application.UseCases.PermisosPersonal.Commands.CreatePermiso;
using Usuarios.Application.UseCases.PermisosPersonal.Commands.DeletePermiso;
using Usuarios.Application.UseCases.PermisosPersonal.Commands.UpdatePermiso;
using Usuarios.Application.UseCases.PermisosPersonal.Queries.GetAllPermisos;
using Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisoById;
using Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisosActivos;
using Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisosByActividad;
using Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisosByHabitacion;
using Usuarios.Application.UseCases.PermisosPersonal.Queries.GetPermisosByPersonal;
using Barcelo.Authorization.Shared;

namespace Usuarios.API.Controllers;

[Authorize]
[HasPermission(Permissions.Usuarios.View)]
[ApiController]
[Route("[controller]")]
public class PermisoPersonalController : ControllerBase
{
    private readonly IMediator _mediator;

    public PermisoPersonalController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllPermisosQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetPermisoByIdQuery(id));
        return Ok(result);
    }

    [HttpGet("personal/{personalId}")]
    public async Task<IActionResult> GetByPersonal(int personalId)
    {
        var result = await _mediator.Send(new GetPermisosByPersonalQuery(personalId));
        return Ok(result);
    }

    [HttpGet("personal/{personalId}/activos")]
    public async Task<IActionResult> GetActivos(int personalId)
    {
        var result = await _mediator.Send(new GetPermisosActivosQuery(personalId));
        return Ok(result);
    }

    [HttpGet("habitacion/{habitacionId}")]
    public async Task<IActionResult> GetByHabitacion(int habitacionId)
    {
        var result = await _mediator.Send(new GetPermisosByHabitacionQuery(habitacionId));
        return Ok(result);
    }

    [HttpGet("actividad/{actividadId}")]
    public async Task<IActionResult> GetByActividad(int actividadId)
    {
        var result = await _mediator.Send(new GetPermisosByActividadQuery(actividadId));
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(Permissions.Usuarios.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePermisosPersonalDto dto)
    {
        var result = await _mediator.Send(new CreatePermisoCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.PermisoId }, result);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.Usuarios.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePermisosPersonalDto dto)
    {
        if (id != dto.PermisoId)
            return BadRequest("El ID no coincide");

        var result = await _mediator.Send(new UpdatePermisoCommand(dto));
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.Usuarios.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeletePermisoCommand(id));
        return Ok(result);
    }
}
