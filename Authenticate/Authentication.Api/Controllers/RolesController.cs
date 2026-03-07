using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            this.roleManager = roleManager;
        }
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await roleManager.Roles.ToListAsync();
            return Ok(roles);
        }

        [HttpPost]
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
    }
}