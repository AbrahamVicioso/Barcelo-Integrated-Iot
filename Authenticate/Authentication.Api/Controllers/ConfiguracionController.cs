using Authentication.Api.DTOs;
using Authentication.Api.Services;
using Barcelo.Authorization.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Authentication.Api.Controllers;

[Route("configuracion/identidad")]
[ApiController]
public class ConfiguracionController(IIdentityConfiguracionService configuracion) : ControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Configuracion.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await configuracion.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("password")]
    [HasPermission(Permissions.Configuracion.View)]
    public async Task<IActionResult> GetPassword(CancellationToken ct)
    {
        var all = await configuracion.GetAllAsync(ct);
        return Ok(all.Password);
    }

    [HttpPut("password")]
    [HasPermission(Permissions.Configuracion.Edit)]
    public async Task<IActionResult> UpdatePassword([FromBody] PasswordPolicyDto dto, CancellationToken ct)
    {
        if (dto.RequiredLength < 6 || dto.RequiredLength > 128)
            return BadRequest(new { error = "RequiredLength debe estar entre 6 y 128." });

        await configuracion.UpdatePasswordPolicyAsync(dto, GetUserId(), ct);
        return Ok(new { message = "Política de contraseñas actualizada." });
    }

    [HttpGet("lockout")]
    [HasPermission(Permissions.Configuracion.View)]
    public async Task<IActionResult> GetLockout(CancellationToken ct)
    {
        var all = await configuracion.GetAllAsync(ct);
        return Ok(all.Lockout);
    }

    [HttpPut("lockout")]
    [HasPermission(Permissions.Configuracion.Edit)]
    public async Task<IActionResult> UpdateLockout([FromBody] LockoutConfigDto dto, CancellationToken ct)
    {
        if (dto.MaxFailedAttempts < 1 || dto.MaxFailedAttempts > 100)
            return BadRequest(new { error = "MaxFailedAttempts debe estar entre 1 y 100." });

        if (dto.LockoutMinutes < 1 || dto.LockoutMinutes > 1440)
            return BadRequest(new { error = "LockoutMinutes debe estar entre 1 y 1440." });

        await configuracion.UpdateLockoutAsync(dto, GetUserId(), ct);
        return Ok(new { message = "Configuración de protección actualizada." });
    }

    [HttpGet("session")]
    [HasPermission(Permissions.Configuracion.View)]
    public async Task<IActionResult> GetSession(CancellationToken ct)
    {
        var all = await configuracion.GetAllAsync(ct);
        return Ok(all.Session);
    }

    [HttpPut("session")]
    [HasPermission(Permissions.Configuracion.Edit)]
    public async Task<IActionResult> UpdateSession([FromBody] SessionConfigDto dto, CancellationToken ct)
    {
        if (dto.TokenExpirationMinutes < 5 || dto.TokenExpirationMinutes > 10080)
            return BadRequest(new { error = "TokenExpirationMinutes debe estar entre 5 y 10080 (7 días)." });

        await configuracion.UpdateSessionAsync(dto, GetUserId(), ct);
        return Ok(new { message = "Gestión de sesiones actualizada." });
    }

    [HttpGet("signin")]
    [HasPermission(Permissions.Configuracion.View)]
    public async Task<IActionResult> GetSignIn(CancellationToken ct)
    {
        var all = await configuracion.GetAllAsync(ct);
        return Ok(all.SignIn);
    }

    [HttpPut("signin")]
    [HasPermission(Permissions.Configuracion.Edit)]
    public async Task<IActionResult> UpdateSignIn([FromBody] SignInConfigDto dto, CancellationToken ct)
    {
        await configuracion.UpdateSignInAsync(dto, GetUserId(), ct);
        return Ok(new { message = "Configuración de verificación actualizada." });
    }

    [HttpGet("twofactor")]
    [HasPermission(Permissions.Configuracion.View)]
    public async Task<IActionResult> GetTwoFactor(CancellationToken ct)
    {
        var all = await configuracion.GetAllAsync(ct);
        return Ok(all.TwoFactor);
    }

    [HttpPut("twofactor")]
    [HasPermission(Permissions.Configuracion.Edit)]
    public async Task<IActionResult> UpdateTwoFactor([FromBody] TwoFactorConfigDto dto, CancellationToken ct)
    {
        await configuracion.UpdateTwoFactorAsync(dto, GetUserId(), ct);
        return Ok(new { message = "Configuración de 2FA actualizada." });
    }

    private string GetUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
}
