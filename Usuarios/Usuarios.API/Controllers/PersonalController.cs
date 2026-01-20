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

namespace Usuarios.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetPersonalByIdQuery(id));
        if (!result.IsSuccess)
            return NotFound(result);
        return Ok(result);
    }

    [HttpGet("activo")]
    public async Task<IActionResult> GetActivo()
    {
        var result = await _mediator.Send(new GetPersonalActivoQuery());
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("departamento/{departamento}")]
    public async Task<IActionResult> GetByDepartamento(string departamento)
    {
        var result = await _mediator.Send(new GetPersonalByDepartamentoQuery(departamento));
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePersonalDto dto)
    {
        var result = await _mediator.Send(new CreatePersonalCommand(dto));
        if (!result.IsSuccess)
            return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.PersonalId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePersonalDto dto)
    {
        if (id != dto.PersonalId)
            return BadRequest("El ID no coincide");

        var result = await _mediator.Send(new UpdatePersonalCommand(dto));
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeletePersonalCommand(id));
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }
}
