using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Barcelo.Authorization.Shared;

namespace Authentication.Api.Controllers;

[Route("[controller]")]
[ApiController]
[HasPermission(Permissions.Roles.View)]
public class RolesController : ControllerBase
{
    private readonly RoleManager<IdentityRole> roleManager;

    public RolesController(RoleManager<IdentityRole> roleManager)
    {
        this.roleManager = roleManager;
    }

    [HttpGet]
    [HasPermission(Permissions.Roles.View)]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await roleManager.Roles.ToListAsync();
        return Ok(roles);
    }

    [HttpPost]
    [HasPermission(Permissions.Roles.Create)]
    public async Task<IActionResult> CreateRole([FromBody] string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return BadRequest("Role name cannot be empty.");
        }

        var existingRole = await roleManager.FindByNameAsync(roleName);
        if (existingRole != null)
        {
            return BadRequest("Role already exists.");
        }

        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
        if (result.Succeeded)
        {
            return Ok($"Role '{roleName}' created successfully.");
        }
        else
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Error creating role.");
        }
    }

    [HttpGet("{roleId}/permissions")]
    [HasPermission(Permissions.Roles.View)]
    public async Task<IActionResult> GetRolePermissions(string roleId)
    {
        var role = await roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            return NotFound("Role not found.");
        }

        var claims = await roleManager.GetClaimsAsync(role);
        var permissions = claims
            .Where(c => c.Type == PermissionConstants.PermissionType)
            .Select(c => c.Value)
            .ToList();

        return Ok(permissions);
    }

    [HttpPost("{roleId}/permissions")]
    [HasPermission(Permissions.Roles.ManagePermissions)]
    public async Task<IActionResult> AddPermissionToRole(string roleId, [FromBody] string permission)
    {
        var role = await roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            return NotFound("Role not found.");
        }

        if (!Permissions.GetAllPermissions().Contains(permission))
        {
            return BadRequest($"Invalid permission '{permission}'.");
        }

        var existingClaim = await roleManager.GetClaimsAsync(role);
        if (existingClaim.Any(c => c.Type == PermissionConstants.PermissionType && c.Value == permission))
        {
            return BadRequest("Permission already exists for this role.");
        }

        var claim = new Claim(PermissionConstants.PermissionType, permission);
        var result = await roleManager.AddClaimAsync(role, claim);

        if (result.Succeeded)
        {
            return Ok(new { message = "Permission added successfully.", permission });
        }

        return StatusCode(StatusCodes.Status500InternalServerError, "Error adding permission.");
    }

    [HttpDelete("{roleId}/permissions/{permission}")]
    [HasPermission(Permissions.Roles.ManagePermissions)]
    public async Task<IActionResult> RemovePermissionFromRole(string roleId, string permission)
    {
        var role = await roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            return NotFound("Role not found.");
        }

        var claims = await roleManager.GetClaimsAsync(role);
        var claim = claims.FirstOrDefault(c => c.Type == PermissionConstants.PermissionType && c.Value == permission);

        if (claim == null)
        {
            return NotFound("Permission not found for this role.");
        }

        var result = await roleManager.RemoveClaimAsync(role, claim);

        if (result.Succeeded)
        {
            return Ok(new { message = "Permission removed successfully.", permission });
        }

        return StatusCode(StatusCodes.Status500InternalServerError, "Error removing permission.");
    }

    [HttpGet("permissions")]
    [HasPermission(Permissions.Roles.ManagePermissions)]
    public IActionResult GetAllPermissions()
    {
        var permissions = PermissionDescriptions.GetAll();
        return Ok(permissions);
    }
}