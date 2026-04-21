using System.Security.Claims;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Barcelo.Authorization.Shared;

namespace Authentication.Api.Data;

public static class DbSeeder
{
    private const string NtfyTokenClaim = "ntfy_token";

    public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager, roleManager, configuration);
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
                Permissions.Usuarios.View, Permissions.Usuarios.Create, Permissions.Usuarios.Edit,
                Permissions.Huespedes.View, Permissions.Huespedes.Create, Permissions.Huespedes.Edit, Permissions.Huespedes.Delete,
                Permissions.Personal.View, Permissions.Personal.Create, Permissions.Personal.Edit, Permissions.Personal.Delete,
                Permissions.Auth.View, Permissions.Auth.Create,
                Permissions.Dispositivos.View, Permissions.Dispositivos.Create, Permissions.Dispositivos.Edit,
                Permissions.Reservas.View, Permissions.Reservas.Create, Permissions.Reservas.Edit,
                Permissions.Habitaciones.View, Permissions.Habitaciones.Create, Permissions.Habitaciones.Edit,
                Permissions.Cerraduras.View, Permissions.Cerraduras.Create, Permissions.Cerraduras.Edit,
                Permissions.Credenciales.View, Permissions.Credenciales.Create, Permissions.Credenciales.Edit,
                Permissions.Hoteles.View, Permissions.Hoteles.Create, Permissions.Hoteles.Edit,
                Permissions.Mantenimientos.View, Permissions.Mantenimientos.Create, Permissions.Mantenimientos.Edit,
                Permissions.Roles.View, Permissions.Roles.Create, Permissions.Roles.Edit,
                Permissions.Reports.View,
                Permissions.Audit.View,
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
                Permissions.Credenciales.View, Permissions.Credenciales.Create,
                Permissions.Hoteles.View,
                Permissions.Huespedes.View, Permissions.Huespedes.Create, Permissions.Huespedes.Edit,
                Permissions.Personal.View,
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

    private static async Task SeedAdminUserAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        const string username = "admin";
        const string email = "admin@barcelo.com";
        const string password = "Admin1234.";

        var admin = await userManager.FindByNameAsync(username);
        if (admin is not null)
        {
            await EnsureNtfyClaimAsync(userManager, admin, configuration);
            return;
        }

        admin = new User
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true,
        };

        var result = await userManager.CreateAsync(admin, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
            await EnsureNtfyClaimAsync(userManager, admin, configuration);
        }
    }

    private static async Task EnsureNtfyClaimAsync(UserManager<User> userManager, User user, IConfiguration configuration)
    {
        var ntfyToken = configuration["Ntfy:AccessToken"] ?? Environment.GetEnvironmentVariable("NTFY_SERVER_TOKEN");
        if (string.IsNullOrEmpty(ntfyToken))
        {
            Console.WriteLine("[DbSeeder] NTFY_SERVER_TOKEN no encontrado en configuración");
            return;
        }

        Console.WriteLine($"[DbSeeder] NTFY_TOKEN encontrado: {ntfyToken.Substring(0, Math.Min(10, ntfyToken.Length))}...");

        var existingClaim = (await userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == NtfyTokenClaim);
        if (existingClaim is null)
        {
            await userManager.AddClaimAsync(user, new Claim(NtfyTokenClaim, ntfyToken));
            Console.WriteLine($"[DbSeeder] Claim ntfy_token agregado al usuario admin");
        }
    }
}