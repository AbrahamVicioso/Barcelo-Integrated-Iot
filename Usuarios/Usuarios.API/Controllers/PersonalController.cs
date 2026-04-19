using MediatR;
using Microsoft.AspNetCore.Mvc;
using Usuarios.Application.DTOs.Personal;
using Usuarios.Application.UseCases.Personal.Commands.CreatePersonal;
using Usuarios.Application.UseCases.Personal.Commands.DeletePersonal;
using Usuarios.Application.UseCases.Personal.Commands.UpdatePersonal;
using Usuarios.Application.UseCases.Personal.Queries.GetAllPersonal;
using Usuarios.Application.UseCases.Personal.Queries.GetPersonalActivo;
using Usuarios.Application.UseCases.Personal.Queries.GetPersonalByDepartamento;
using Usuarios.Application.UseCases.Personal.Queries.GetPersonalById;
using Usuarios.Application.UseCases.Personal.Queries.GetPersonalByUserId;
using Barcelo.Authorization.Shared;

namespace Usuarios.API.Controllers;

[ApiController]
[HasPermission(Permissions.Usuarios.View)]
[Route("[controller]")]
public class PersonalController : ControllerBase
{
    private readonly IMediator _mediator;

    public PersonalController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllPersonalQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetPersonalByIdQuery(id));
        return Ok(result);
    }

    [HttpGet("activo")]
    public async Task<IActionResult> GetActivo()
    {
        var result = await _mediator.Send(new GetPersonalActivoQuery());
        return Ok(result);
    }

    [HttpGet("user/{usuarioId}")]
    public async Task<IActionResult> GetByUserId(string usuarioId)
    {
        var result = await _mediator.Send(new GetPersonalByUserIdQuery(usuarioId));
        return Ok(result);
    }

    [HttpGet("departamento/{departamentoId:int}")]
    public async Task<IActionResult> GetByDepartamento(int departamentoId)
    {
        var result = await _mediator.Send(new GetPersonalByDepartamentoQuery(departamentoId));
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(Permissions.Usuarios.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePersonalDto dto)
    {
        var result = await _mediator.Send(new CreatePersonalCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.PersonalId }, result);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.Usuarios.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePersonalDto dto)
    {
        if (id != dto.PersonalId)
            return BadRequest("El ID no coincide");

        var result = await _mediator.Send(new UpdatePersonalCommand(dto));
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.Usuarios.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeletePersonalCommand(id));
        return Ok(result);
    }
}
