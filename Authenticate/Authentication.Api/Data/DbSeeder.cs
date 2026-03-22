using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "Recepcionista", "Mantenimiento"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<User> userManager)
    {
        
        const string username = "admin@barcelo.com";
        const string email 
          = "admin@barcelo.com";
        const string password = "Admin1234.";  // Min. 6 caracteres requeridos por Identity

        if (await userManager.FindByNameAsync(username) is not null)
            return;

        var admin = new User
        {

            UserName = username,
            Email    = email,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(admin, password);

        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}
