using System.ComponentModel.DataAnnotations;

namespace Authentication.Api.DTOs;

// DTO para crear usuario
public class CreateUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string UserName { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
}

// DTO para actualizar usuario
public class UpdateUserDto
{
    [Required]
    public string Id { get; set; } = string.Empty;
    
    [EmailAddress]
    public string? Email { get; set; }
    
    public string? UserName { get; set; }
    
    public string? PhoneNumber { get; set; }
}

// DTO para respuesta de usuario
public class UserResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Claims { get; set; } = new();
}

// DTO para cambio de contraseña
public class ChangePasswordDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}

// DTO para agregar/remover rol
public class UserRoleDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string RoleName { get; set; } = string.Empty;
}

// DTO para agregar claim
public class UserClaimDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string ClaimType { get; set; } = string.Empty;
    
    [Required]
    public string ClaimValue { get; set; } = string.Empty;
}

// DTO para paginación
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
