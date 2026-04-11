using MediatR;
using Microsoft.AspNetCore.Mvc;
using Usuarios.Application.DTOs.Departamento;
using Usuarios.Application.UseCases.Departamento.Commands.CreateDepartamento;
using Usuarios.Application.UseCases.Departamento.Commands.DeleteDepartamento;
using Usuarios.Application.UseCases.Departamento.Commands.UpdateDepartamento;
using Usuarios.Application.UseCases.Departamento.Queries.GetAllDepartamentos;
using Usuarios.Application.UseCases.Departamento.Queries.GetDepartamentoById;

namespace Usuarios.API.Controllers;

[ApiController]
[Route("[controller]")]
public class DepartamentoController : ControllerBase
{
    private readonly IMediator _mediator;

    public DepartamentoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllDepartamentosQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetDepartamentoByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDepartamentoDto dto)
    {
        var result = await _mediator.Send(new CreateDepartamentoCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.DepartamentoId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartamentoDto dto)
    {
        if (id != dto.DepartamentoId)
            return BadRequest("El ID no coincide");

        var result = await _mediator.Send(new UpdateDepartamentoCommand(dto));
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteDepartamentoCommand(id));
        return Ok(result);
    }
}
