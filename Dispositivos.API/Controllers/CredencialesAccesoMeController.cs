using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Dispositivos.Application.Common;
using Dispositivos.Application.Features.CredencialesAcceso.Commands;
using Dispositivos.Application.Features.CredencialesAcceso.Queries;

namespace Dispositivos.API.Controllers;

[Route("credencialesacceso/me")]
[ApiController]
[Authorize]
public class CredencialesAccesoMeController : ControllerBase
{
    private readonly IMediator _mediator;

    public CredencialesAccesoMeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string? GetUsuarioId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("nameid");

    [HttpGet("personal")]
    public async Task<IActionResult> GetMePersonal([FromQuery] PaginationParams pagination)
    {
        var usuarioId = GetUsuarioId();
        if (string.IsNullOrEmpty(usuarioId))
            return Unauthorized();

        var result = await _mediator.Send(new GetCredencialesMePersonalQuery
        {
            UsuarioId = usuarioId,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        });

        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("huesped")]
    public async Task<IActionResult> GetMeHuesped([FromQuery] PaginationParams pagination)
    {
        var usuarioId = GetUsuarioId();
        if (string.IsNullOrEmpty(usuarioId))
            return Unauthorized();

        var result = await _mediator.Send(new GetCredencialesMeHuespedQuery
        {
            UsuarioId = usuarioId,
            Page = pagination.Page,
            PageSize = pagination.PageSize
        });

        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPatch("personal/{credencialId}/toggle")]
    public async Task<IActionResult> ToggleMePersonal(int credencialId)
    {
        var usuarioId = GetUsuarioId();
        if (string.IsNullOrEmpty(usuarioId))
            return Unauthorized();

        var result = await _mediator.Send(new ToggleCredencialMePersonalCommand
        {
            CredencialId = credencialId,
            UsuarioId = usuarioId
        });

        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpPatch("huesped/{credencialId}/toggle")]
    public async Task<IActionResult> ToggleMeHuesped(int credencialId)
    {
        var usuarioId = GetUsuarioId();
        if (string.IsNullOrEmpty(usuarioId))
            return Unauthorized();

        var result = await _mediator.Send(new ToggleCredencialMeHuespedCommand
        {
            CredencialId = credencialId,
            UsuarioId = usuarioId
        });

        if (!result.IsSuccess)
            return result.IsNotFound ? NotFound(new { error = result.ErrorMessage }) : BadRequest(new { error = result.ErrorMessage });
        return Ok(result.Data);
    }
}
