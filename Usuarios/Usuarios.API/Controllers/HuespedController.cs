using MediatR;
using Microsoft.AspNetCore.Mvc;
using Usuarios.Application.DTOs.Huespedes;
using Usuarios.Application.UseCases.Huespedes.Commands.CreateHuespede;
using Usuarios.Application.UseCases.Huespedes.Commands.DeleteHuespede;
using Usuarios.Application.UseCases.Huespedes.Commands.UpdateHuespede;
using Usuarios.Application.UseCases.Huespedes.Queries.GetAllHuespedes;
using Usuarios.Application.UseCases.Huespedes.Queries.GetHuespedeById;
using Usuarios.Application.UseCases.Huespedes.Queries.GetHuespedesVip;

namespace Usuarios.API.Controllers;

[ApiController]
[Route("[controller]")]
public class HuespedController : ControllerBase
{
    private readonly IMediator _mediator;

    public HuespedController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllHuespedesQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetHuespedeByIdQuery(id));
        return Ok(result);
    }

    [HttpGet("vip")]
    public async Task<IActionResult> GetVip()
    {
        var result = await _mediator.Send(new GetHuespedesVipQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHuespedeDto dto)
    {
        var result = await _mediator.Send(new CreateHuespedeCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.HuespedId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateHuespedeDto dto)
    {
        if (id != dto.HuespedId)
            return BadRequest("El ID no coincide");

        var result = await _mediator.Send(new UpdateHuespedeCommand(dto));
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteHuespedeCommand(id));
        return Ok(result);
    }
}
