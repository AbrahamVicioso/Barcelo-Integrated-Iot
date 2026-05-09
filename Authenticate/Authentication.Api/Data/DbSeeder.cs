using System.Security.Claims;
using Authentication.Api.Entities;
using Authentication.Api.Services;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Barcelo.Authorization.Shared;

namespace Authentication.Api.Data;

public static class DbSeeder
{
    private const string NtfyTokenClaim = "ntfy_token";

    public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, AuthenticationDbContext db)
    {
        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager, roleManager, configuration);
        await SeedConfiguracionDefaultsAsync(db);
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

    private static async Task SeedConfiguracionDefaultsAsync(AuthenticationDbContext db)
    {
        var defaults = new (string Clave, string Valor, string Tipo, string Descripcion)[]
        {
            ("Auth.Password.RequiredLength",        "8",     "int",  "Longitud mínima de contraseña"),
            ("Auth.Password.RequireUppercase",       "true",  "bool", "Requerir al menos una letra mayúscula"),
            ("Auth.Password.RequireDigit",           "true",  "bool", "Requerir al menos un número"),
            ("Auth.Password.RequireNonAlphanumeric", "true",  "bool", "Requerir al menos un carácter especial"),
            ("Auth.Lockout.MaxFailedAttempts",       "5",     "int",  "Intentos fallidos antes de bloqueo"),
            ("Auth.Lockout.LockoutMinutes",          "15",    "int",  "Minutos de bloqueo tras superar intentos"),
            ("Auth.Session.TokenExpirationMinutes",  "30",    "int",  "Minutos de expiración del token JWT"),
            ("Auth.SignIn.RequireConfirmedEmail",     "false", "bool", "Requerir confirmación de correo antes de login"),
            ("Auth.SignIn.AllowPasswordReset",        "true",  "bool", "Permitir restablecimiento de contraseña"),
            ("Auth.TwoFactor.RequireForAdmins",      "false", "bool", "Requerir 2FA para usuarios con rol Admin"),
        };

        foreach (var (clave, valor, tipo, descripcion) in defaults)
        {
            var exists = await db.ConfiguracionSistema.AnyAsync(c => c.HotelId == null && c.Clave == clave);
            if (!exists)
            {
                db.ConfiguracionSistema.Add(new ConfiguracionSistema
                {
                    Clave         = clave,
                    Valor         = valor,
                    TipoDato      = tipo,
                    Descripcion   = descripcion,
                    EsGlobal      = true,
                    FechaCreacion = DateTime.Now,
                    ModificadoPor = "system"
                });
            }
        }

        await db.SaveChangesAsync();
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