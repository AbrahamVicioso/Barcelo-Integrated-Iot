using MediatR;
using Microsoft.AspNetCore.Mvc;
using Usuarios.Application.DTOs.Puesto;
using Usuarios.Application.UseCases.Puesto.Commands.CreatePuesto;
using Usuarios.Application.UseCases.Puesto.Commands.DeletePuesto;
using Usuarios.Application.UseCases.Puesto.Commands.UpdatePuesto;
using Usuarios.Application.UseCases.Puesto.Queries.GetAllPuestos;
using Usuarios.Application.UseCases.Puesto.Queries.GetPuestoById;
using Barcelo.Authorization.Shared;

namespace Usuarios.API.Controllers;

[ApiController]
[HasPermission(Permissions.Usuarios.View)]
[Route("[controller]")]
public class PuestoController : ControllerBase
{
    private readonly IMediator _mediator;

    public PuestoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllPuestosQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetPuestoByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(Permissions.Usuarios.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePuestoDto dto)
    {
        var result = await _mediator.Send(new CreatePuestoCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.PuestoId }, result);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.Usuarios.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePuestoDto dto)
    {
        if (id != dto.PuestoId)
            return BadRequest("El ID no coincide");

        var result = await _mediator.Send(new UpdatePuestoCommand(dto));
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.Usuarios.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeletePuestoCommand(id));
        return Ok(result);
    }
}
