using Authentication.Api.DTOs;
using Authentication.Api.Services;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Notification.Domain.Events;
using System.Text;
using Barcelo.Authorization.Shared;

namespace Authentication.Api.Controllers;

[Route("[controller]")]
[ApiController]
[HasPermission(Permissions.Usuarios.View)]
public class UsersController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IKafkaProducerService _kafkaProducerService;
    private readonly IConfiguration _configuration;

    public UsersController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IKafkaProducerService kafkaProducerService, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _kafkaProducerService = kafkaProducerService;
        _configuration = configuration;
    }

    #region Queries

    /// <summary>
    /// Obtiene todos los usuarios con paginación
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserResponseDto>>> GetAll(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10)
    {
        var query = _userManager.Users;
        
        var totalCount = await query.CountAsync();
        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userResponses = new List<UserResponseDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            
            userResponses.Add(new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount,
                Roles = roles.ToList(),
                Claims = claims.Select(c => $"{c.Type}:{c.Value}").ToList()
            });
        }

        return Ok(new PagedResult<UserResponseDto>
        {
            Items = userResponses,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    /// <summary>
    /// Obtiene un usuario por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var response = new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            LockoutEnabled = user.LockoutEnabled,
            LockoutEnd = user.LockoutEnd,
            AccessFailedCount = user.AccessFailedCount,
            Roles = roles.ToList(),
            Claims = claims.Select(c => $"{c.Type}:{c.Value}").ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Obtiene un usuario por email
    /// </summary>
    [HttpGet("by-email/{email}")]
    public async Task<ActionResult<UserResponseDto>> GetByEmail(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound(new { message = $"Usuario con email {email} no encontrado" });

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var response = new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            LockoutEnabled = user.LockoutEnabled,
            LockoutEnd = user.LockoutEnd,
            AccessFailedCount = user.AccessFailedCount,
            Roles = roles.ToList(),
            Claims = claims.Select(c => $"{c.Type}:{c.Value}").ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Obtiene los roles de un usuario
    /// </summary>
    [HttpGet("{id}/roles")]
    public async Task<ActionResult<List<string>>> GetUserRoles(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(roles.ToList());
    }

    /// <summary>
    /// Obtiene todos los roles disponibles
    /// </summary>
    [HttpGet("roles")]
    public async Task<ActionResult<List<string>>> GetAllRoles()
    {
        var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        return Ok(roles);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Crea un nuevo usuario
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Usuarios.Create)]
    public async Task<ActionResult<UserResponseDto>> Create([FromBody] CreateUserDto model)
    {
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
            return BadRequest(new { message = "Ya existe un usuario con este email" });

        var existingUserName = await _userManager.FindByNameAsync(model.UserName);
        if (existingUserName != null)
            return BadRequest(new { message = "Ya existe un usuario con este nombre de usuario" });

        var user = new User
        {
            Email = model.Email,
            UserName = model.UserName,
            PhoneNumber = model.PhoneNumber
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { message = "Error al crear usuario", errors });
        }

        // Publish UserCreatedEvent to Kafka for welcome email + ntfy setup
        var userCreatedEvent = new UserCreatedEvent
        {
            Id = Guid.Parse(user.Id),
            Email = model.Email,
            GeneratedPassword = model.Password,
            UserName = model.UserName,
            CreatedAt = DateTime.Now
        };

        await _kafkaProducerService.PublishUserCreatedAsync(userCreatedEvent);

        // Publish EmailConfirmationEvent so the user must confirm their email
        var confirmEmailBaseUrl = BuildConfirmEmailBaseUrl();
        var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmToken));
        var confirmationUrl = $"{confirmEmailBaseUrl}/ConfirmEmail?userId={user.Id}&token={encodedToken}";

        await _kafkaProducerService.PublishEmailConfirmationAsync(new EmailConfirmationEvent
        {
            UserId = user.Id,
            Email = user.Email!,
            ConfirmationUrl = confirmationUrl
        });

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            LockoutEnabled = user.LockoutEnabled,
            LockoutEnd = user.LockoutEnd,
            AccessFailedCount = user.AccessFailedCount,
            Roles = new List<string>(),
            Claims = new List<string>()
        });
    }

    /// <summary>
    /// Actualiza un usuario existente
    /// </summary>
    [HttpPut("{id}")]
    [HasPermission(Permissions.Usuarios.Edit)]
    public async Task<ActionResult<UserResponseDto>> Update(string id, [FromBody] UpdateUserDto model)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });

        if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
        {
            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null && existingEmail.Id != id)
                return BadRequest(new { message = "Ya existe otro usuario con este email" });
            
            user.Email = model.Email;
        }

        if (!string.IsNullOrEmpty(model.UserName) && model.UserName != user.UserName)
        {
            var existingUserName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserName != null && existingUserName.Id != id)
                return BadRequest(new { message = "Ya existe otro usuario con este nombre de usuario" });
            
            user.UserName = model.UserName;
        }

        if (model.PhoneNumber != null)
        {
            user.PhoneNumber = model.PhoneNumber;
        }

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { message = "Error al actualizar usuario", errors });
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            LockoutEnabled = user.LockoutEnabled,
            LockoutEnd = user.LockoutEnd,
            AccessFailedCount = user.AccessFailedCount,
            Roles = roles.ToList(),
            Claims = new List<string>()
        });
    }

    /// <summary>
    /// Elimina un usuario
    /// </summary>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.Usuarios.Delete)]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { message = "Error al eliminar usuario", errors });
        }

        return NoContent();
    }

    /// <summary>
    /// Agrega un rol a un usuario
    /// </summary>
    [HttpPost("{id}/roles")]
    [HasPermission(Permissions.Roles.Edit)]
    public async Task<IActionResult> AddToRole(string id, [FromBody] UserRoleDto model)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });

        var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
        if (!roleExists)
            return BadRequest(new { message = $"El rol {model.RoleName} no existe" });

        var isInRole = await _userManager.IsInRoleAsync(user, model.RoleName);
        if (isInRole)
            return BadRequest(new { message = $"El usuario ya tiene el rol {model.RoleName}" });

        var result = await _userManager.AddToRoleAsync(user, model.RoleName);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { message = "Error al agregar rol", errors });
        }

        return Ok(new { message = $"Rol {model.RoleName} agregado exitosamente" });
    }

    /// <summary>
    /// Remueve un rol de un usuario
    /// </summary>
    [HttpDelete("{id}/roles/{roleName}")]
    [HasPermission(Permissions.Roles.Edit)]
    public async Task<IActionResult> RemoveFromRole(string id, string roleName)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });

        var isInRole = await _userManager.IsInRoleAsync(user, roleName);
        if (!isInRole)
            return BadRequest(new { message = $"El usuario no tiene el rol {roleName}" });

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { message = "Error al remover rol", errors });
        }

        return Ok(new { message = $"Rol {roleName} removido exitosamente" });
    }

    /// <summary>
    /// Bloquea a un usuario por un tiempo especificado
    /// </summary>
    [HttpPost("{id}/lock")]
    [HasPermission(Permissions.Usuarios.Edit)]
    public async Task<IActionResult> LockUser(string id, [FromQuery] int days = 0, [FromQuery] int hours = 0, [FromQuery] int minutes = 0)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });

        var lockoutEnd = DateTimeOffset.UtcNow.AddDays(days).AddHours(hours).AddMinutes(minutes);
        var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { message = "Error al bloquear usuario", errors });
        }

        // Reset failed access count
        await _userManager.ResetAccessFailedCountAsync(user);

        return Ok(new { message = $"Usuario bloqueado hasta {lockoutEnd}" });
    }

    /// <summary>
    /// Desbloquea a un usuario
    /// </summary>
    [HttpPost("{id}/unlock")]
    [HasPermission(Permissions.Usuarios.Edit)]
    public async Task<IActionResult> UnlockUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });

        var result = await _userManager.SetLockoutEndDateAsync(user, null);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { message = "Error al desbloquear usuario", errors });
        }

        // Reset failed access count
        await _userManager.ResetAccessFailedCountAsync(user);

        return Ok(new { message = "Usuario desbloqueado exitosamente" });
    }

    /// <summary>
    /// Cambia la contraseña de un usuario
    /// </summary>
    [HttpPost("{id}/change-password")]
    [HasPermission(Permissions.Usuarios.Edit)]
    public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordDto model)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { message = "Error al cambiar contraseña", errors });
        }

        return Ok(new { message = "Contraseña cambiada exitosamente" });
    }

    /// <summary>
    /// Establece una contraseña para un usuario (sin necesidad de conocer la anterior)
    /// </summary>
    [HttpPost("{id}/set-password")]
    [HasPermission(Permissions.Usuarios.Edit)]
    public async Task<IActionResult> SetPassword(string id, [FromBody] ChangePasswordDto model)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });

        // Check if user already has a password
        var hasPassword = await _userManager.HasPasswordAsync(user);
        if (hasPassword)
        {
            // Use password change flow
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { message = "Error al establecer contraseña", errors });
            }
        }
        else
        {
            // Add password directly
            var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
            
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { message = "Error al establecer contraseña", errors });
            }
        }

        return Ok(new { message = "Contraseña establecida exitosamente" });
    }

    /// <summary>
    /// Confirma el email de un usuario
    /// </summary>
    [HttpPost("{id}/confirm-email")]
    [HasPermission(Permissions.Usuarios.Edit)]
    public async Task<IActionResult> ConfirmEmail(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });

        if (user.EmailConfirmed)
            return BadRequest(new { message = "El email ya está confirmado" });

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { message = "Error al confirmar email", errors });
        }

        return Ok(new { message = "Email confirmado exitosamente" });
    }

    /// <summary>
    /// Agrega un claim a un usuario
    /// </summary>
    [HttpPost("{id}/claims")]
    [HasPermission(Permissions.Roles.ManagePermissions)]
    public async Task<IActionResult> AddClaim(string id, [FromBody] UserClaimDto model)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });

        var claim = new System.Security.Claims.Claim(model.ClaimType, model.ClaimValue);
        var result = await _userManager.AddClaimAsync(user, claim);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { message = "Error al agregar claim", errors });
        }

        return Ok(new { message = "Claim agregado exitosamente" });
    }

    /// <summary>
    /// Remueve un claim de un usuario
    /// </summary>
    [HttpDelete("{id}/claims")]
    [HasPermission(Permissions.Roles.ManagePermissions)]
    public async Task<IActionResult> RemoveClaim(string id, [FromQuery] string claimType, [FromQuery] string claimValue)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });

        var claim = new System.Security.Claims.Claim(claimType, claimValue);
        var result = await _userManager.RemoveClaimAsync(user, claim);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { message = "Error al remover claim", errors });
        }

        return Ok(new { message = "Claim removido exitosamente" });
    }

    /// <summary>
    /// Obtiene los claims de un usuario
    /// </summary>
    [HttpGet("{id}/claims")]
    public async Task<ActionResult<List<string>>> GetUserClaims(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new { message = $"Usuario con ID {id} no encontrado" });

        var claims = await _userManager.GetClaimsAsync(user);
        return Ok(claims.Select(c => $"{c.Type}:{c.Value}").ToList());
    }

    #endregion

    private string BuildConfirmEmailBaseUrl()
    {
        var forwardedHost = Request.Headers["X-Forwarded-Host"].FirstOrDefault();
        var forwardedProto = Request.Headers["X-Forwarded-Proto"].FirstOrDefault();

        if (!string.IsNullOrEmpty(forwardedHost))
        {
            var proto = forwardedProto ?? Request.Scheme;
            var pathPrefix = _configuration["ConfirmEmail:PathPrefix"] ?? "/api/auth";
            return $"{proto}://{forwardedHost}{pathPrefix}";
        }

        return _configuration["ConfirmEmail:BaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
    }
}
