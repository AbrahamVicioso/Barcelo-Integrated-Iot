using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Usuarios.Application.DTOs.Huespedes;
using Usuarios.Application.UseCases.Huespedes.Commands.CreateHuespede;
using Usuarios.Application.UseCases.Huespedes.Commands.CreateHuespedeMe;
using Usuarios.Application.UseCases.Huespedes.Commands.DeleteHuespede;
using Usuarios.Application.UseCases.Huespedes.Commands.UpdateHuespede;
using Usuarios.Application.UseCases.Huespedes.Queries.GetAllHuespedes;
using Usuarios.Application.UseCases.Huespedes.Queries.GetHuespedeById;
using Usuarios.Application.UseCases.Huespedes.Queries.GetHuespedeByUserId;
using Usuarios.Application.UseCases.Huespedes.Queries.GetHuespedesVip;
using Barcelo.Authorization.Shared;

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
    [HasPermission(Permissions.Usuarios.View)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllHuespedesQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    [HasPermission(Permissions.Usuarios.View)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetHuespedeByIdQuery(id));
        return Ok(result);
    }

    [HttpGet("vip")]
    [HasPermission(Permissions.Usuarios.View)]
    public async Task<IActionResult> GetVip()
    {
        var result = await _mediator.Send(new GetHuespedesVipQuery());
        return Ok(result);
    }

    [HttpGet("user/{usuarioId}")]
    [HasPermission(Permissions.Usuarios.View)]
    public async Task<IActionResult> GetByUserId(string usuarioId)
    {
        var result = await _mediator.Send(new GetHuespedeByUserIdQuery(usuarioId));
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var usuarioId = User.FindFirstValue("nameid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (usuarioId == null)
            return Unauthorized();

        var result = await _mediator.Send(new GetHuespedeByUserIdQuery(usuarioId));
        return Ok(result);
    }

    [Authorize]
    [HttpPost("me")]
    public async Task<IActionResult> CreateMe([FromBody] SelfCreateHuespedeDto dto)
    {
        var usuarioId = User.FindFirstValue("nameid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (usuarioId == null)
            return Unauthorized();

        var result = await _mediator.Send(new CreateHuespedeMeCommand(usuarioId, dto));
        return CreatedAtAction(nameof(GetById), new { id = result.HuespedId }, result);
    }

    [HttpPost]
    [HasPermission(Permissions.Usuarios.Create)]
    public async Task<IActionResult> Create([FromBody] CreateHuespedeDto dto)
    {
        var result = await _mediator.Send(new CreateHuespedeCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.HuespedId }, result);
    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.Usuarios.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateHuespedeDto dto)
    {
        if (id != dto.HuespedId)
            return BadRequest("El ID no coincide");

        var result = await _mediator.Send(new UpdateHuespedeCommand(dto));
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [HasPermission(Permissions.Usuarios.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteHuespedeCommand(id));
        return Ok(result);
    }
}
