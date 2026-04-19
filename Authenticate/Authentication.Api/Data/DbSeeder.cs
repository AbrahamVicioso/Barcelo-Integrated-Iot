using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Barcelo.Authorization.Shared;

namespace Authentication.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager, roleManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var allPermissions = Permissions.GetAllPermissions();

        var adminRole = new IdentityRole("Admin");
        if (await roleManager.FindByNameAsync("Admin") is null)
        {
            await roleManager.CreateAsync(adminRole);
            foreach (var permission in allPermissions)
            {
                await roleManager.AddClaimAsync(adminRole, new System.Security.Claims.Claim(PermissionConstants.PermissionType, permission));
            }
        }

        var managerRole = new IdentityRole("Manager");
        if (await roleManager.FindByNameAsync("Manager") is null)
        {
            await roleManager.CreateAsync(managerRole);
            var managerPermissions = new[]
            {
                Permissions.Dispositivos.View, Permissions.Dispositivos.Create, Permissions.Dispositivos.Edit,
                Permissions.Reservas.View, Permissions.Reservas.Create, Permissions.Reservas.Edit,
                Permissions.Habitaciones.View, Permissions.Habitaciones.Create, Permissions.Habitaciones.Edit,
                Permissions.Cerraduras.View, Permissions.Cerraduras.Create, Permissions.Cerraduras.Edit,
                Permissions.Credenciales.View, Permissions.Credenciales.Create, Permissions.Credenciales.Edit,
                Permissions.Hoteles.View, Permissions.Hoteles.Create, Permissions.Hoteles.Edit,
                Permissions.Reports.View,
            };
            foreach (var permission in managerPermissions)
            {
                await roleManager.AddClaimAsync(managerRole, new System.Security.Claims.Claim(PermissionConstants.PermissionType, permission));
            }
        }

        var recepcionistRole = new IdentityRole("Recepcionist");
        if (await roleManager.FindByNameAsync("Recepcionist") is null)
        {
            await roleManager.CreateAsync(recepcionistRole);
            var recepcionistPermissions = new[]
            {
                Permissions.Reservas.View, Permissions.Reservas.Create, Permissions.Reservas.Edit,
                Permissions.Habitaciones.View,
                Permissions.Cerraduras.View, Permissions.Cerraduras.Create,
                Permissions.Credenciales.View, Permissions.Credenciales.Create,
            };
            foreach (var permission in recepcionistPermissions)
            {
                await roleManager.AddClaimAsync(recepcionistRole, new System.Security.Claims.Claim(PermissionConstants.PermissionType, permission));
            }
        }

        var guestRole = new IdentityRole("Guest");
        if (await roleManager.FindByNameAsync("Guest") is null)
        {
            await roleManager.CreateAsync(guestRole);
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        const string username = "admin";
        const string email = "admin@barcelo.com";
        const string password = "Admin1234.";

        var existingUser = await userManager.FindByNameAsync(username);
        if (existingUser is not null)
        {
            if (!await userManager.IsInRoleAsync(existingUser, "Admin"))
            {
                await userManager.AddToRoleAsync(existingUser, "Admin");
            }
            return;
        }

        var admin = new User
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(admin, password);

        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}