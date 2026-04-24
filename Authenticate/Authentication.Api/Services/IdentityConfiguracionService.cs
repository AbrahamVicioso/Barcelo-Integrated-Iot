using Authentication.Api.Data;
using Authentication.Api.DTOs;
using Authentication.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Authentication.Api.Services;

public class IdentityConfiguracionService(
    AuthenticationDbContext db,
    IOptions<IdentityOptions> identityOptions,
    IdentityRuntimeSettings runtimeSettings) : IIdentityConfiguracionService
{
    private const string Prefix = "Auth.";

    // ─── claves ────────────────────────────────────────────────────────────────
    private static class Keys
    {
        public const string PasswordLength         = "Auth.Password.RequiredLength";
        public const string PasswordUppercase      = "Auth.Password.RequireUppercase";
        public const string PasswordDigit          = "Auth.Password.RequireDigit";
        public const string PasswordSpecial        = "Auth.Password.RequireNonAlphanumeric";
        public const string LockoutAttempts        = "Auth.Lockout.MaxFailedAttempts";
        public const string LockoutMinutes         = "Auth.Lockout.LockoutMinutes";
        public const string SignInEmail            = "Auth.SignIn.RequireConfirmedEmail";
        public const string SignInPasswordReset    = "Auth.SignIn.AllowPasswordReset";
        public const string TwoFactorAdmins        = "Auth.TwoFactor.RequireForAdmins";
        public const string SessionExpiry          = "Auth.Session.TokenExpirationMinutes";
    }

    // ─── public ────────────────────────────────────────────────────────────────

    public async Task<ConfiguracionIdentidadDto> GetAllAsync(CancellationToken ct = default)
    {
        var map = await LoadMapAsync(ct);
        return BuildDto(map);
    }

    public async Task UpdatePasswordPolicyAsync(PasswordPolicyDto dto, string modifiedBy, CancellationToken ct = default)
    {
        await UpsertAsync(Keys.PasswordLength,    dto.RequiredLength.ToString(),         "int",  "Longitud mínima de contraseña",                  modifiedBy, ct);
        await UpsertAsync(Keys.PasswordUppercase, dto.RequireUppercase.ToString(),       "bool", "Requerir al menos una letra mayúscula",           modifiedBy, ct);
        await UpsertAsync(Keys.PasswordDigit,     dto.RequireDigit.ToString(),           "bool", "Requerir al menos un número",                    modifiedBy, ct);
        await UpsertAsync(Keys.PasswordSpecial,   dto.RequireNonAlphanumeric.ToString(), "bool", "Requerir al menos un carácter especial",          modifiedBy, ct);
        await db.SaveChangesAsync(ct);

        ApplyPasswordPolicy(dto);
    }

    public async Task UpdateLockoutAsync(LockoutConfigDto dto, string modifiedBy, CancellationToken ct = default)
    {
        await UpsertAsync(Keys.LockoutAttempts, dto.MaxFailedAttempts.ToString(), "int", "Intentos fallidos antes de bloqueo",       modifiedBy, ct);
        await UpsertAsync(Keys.LockoutMinutes,  dto.LockoutMinutes.ToString(),    "int", "Minutos de bloqueo tras superar intentos", modifiedBy, ct);
        await db.SaveChangesAsync(ct);

        ApplyLockout(dto);
    }

    public async Task UpdateSessionAsync(SessionConfigDto dto, string modifiedBy, CancellationToken ct = default)
    {
        await UpsertAsync(Keys.SessionExpiry, dto.TokenExpirationMinutes.ToString(), "int", "Minutos de expiración del token JWT", modifiedBy, ct);
        await db.SaveChangesAsync(ct);

        runtimeSettings.TokenExpirationMinutes = dto.TokenExpirationMinutes;
    }

    public async Task UpdateSignInAsync(SignInConfigDto dto, string modifiedBy, CancellationToken ct = default)
    {
        await UpsertAsync(Keys.SignInEmail,         dto.RequireConfirmedEmail.ToString(), "bool", "Requerir confirmación de correo antes de login", modifiedBy, ct);
        await UpsertAsync(Keys.SignInPasswordReset, dto.AllowPasswordReset.ToString(),    "bool", "Permitir restablecimiento de contraseña",        modifiedBy, ct);
        await db.SaveChangesAsync(ct);

        identityOptions.Value.SignIn.RequireConfirmedEmail = dto.RequireConfirmedEmail;
        runtimeSettings.AllowPasswordReset = dto.AllowPasswordReset;
    }

    public async Task UpdateTwoFactorAsync(TwoFactorConfigDto dto, string modifiedBy, CancellationToken ct = default)
    {
        await UpsertAsync(Keys.TwoFactorAdmins, dto.RequireForAdmins.ToString(), "bool", "Requerir 2FA para usuarios con rol Admin", modifiedBy, ct);
        await db.SaveChangesAsync(ct);

        runtimeSettings.TwoFactorRequireForAdmins = dto.RequireForAdmins;
    }

    public async Task ApplyAllToIdentityAsync(CancellationToken ct = default)
    {
        var map = await LoadMapAsync(ct);
        var dto = BuildDto(map);

        ApplyPasswordPolicy(dto.Password);
        ApplyLockout(dto.Lockout);
        identityOptions.Value.SignIn.RequireConfirmedEmail = dto.SignIn.RequireConfirmedEmail;

        runtimeSettings.TokenExpirationMinutes      = dto.Session.TokenExpirationMinutes;
        runtimeSettings.AllowPasswordReset          = dto.SignIn.AllowPasswordReset;
        runtimeSettings.TwoFactorRequireForAdmins   = dto.TwoFactor.RequireForAdmins;
    }

    // ─── private helpers ───────────────────────────────────────────────────────

    private async Task<Dictionary<string, string>> LoadMapAsync(CancellationToken ct)
    {
        var rows = await db.ConfiguracionSistema
            .AsNoTracking()
            .Where(c => c.HotelId == null && c.Clave.StartsWith(Prefix))
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.Clave, r => r.Valor);
    }

    private static ConfiguracionIdentidadDto BuildDto(Dictionary<string, string> map) => new(
        Password: new PasswordPolicyDto(
            RequiredLength:        GetInt(map,  Keys.PasswordLength,    8),
            RequireUppercase:      GetBool(map, Keys.PasswordUppercase, true),
            RequireDigit:          GetBool(map, Keys.PasswordDigit,     true),
            RequireNonAlphanumeric: GetBool(map, Keys.PasswordSpecial,  true)),
        Lockout: new LockoutConfigDto(
            MaxFailedAttempts: GetInt(map, Keys.LockoutAttempts, 5),
            LockoutMinutes:    GetInt(map, Keys.LockoutMinutes,  15)),
        Session: new SessionConfigDto(
            TokenExpirationMinutes: GetInt(map, Keys.SessionExpiry, 30)),
        SignIn: new SignInConfigDto(
            RequireConfirmedEmail: GetBool(map, Keys.SignInEmail,         false),
            AllowPasswordReset:    GetBool(map, Keys.SignInPasswordReset, true)),
        TwoFactor: new TwoFactorConfigDto(
            RequireForAdmins: GetBool(map, Keys.TwoFactorAdmins, false)));

    private void ApplyPasswordPolicy(PasswordPolicyDto dto)
    {
        identityOptions.Value.Password.RequiredLength        = dto.RequiredLength;
        identityOptions.Value.Password.RequireUppercase      = dto.RequireUppercase;
        identityOptions.Value.Password.RequireDigit          = dto.RequireDigit;
        identityOptions.Value.Password.RequireNonAlphanumeric = dto.RequireNonAlphanumeric;
    }

    private void ApplyLockout(LockoutConfigDto dto)
    {
        identityOptions.Value.Lockout.MaxFailedAccessAttempts = dto.MaxFailedAttempts;
        identityOptions.Value.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(dto.LockoutMinutes);
    }

    private async Task UpsertAsync(string clave, string valor, string tipo, string descripcion, string modifiedBy, CancellationToken ct)
    {
        var existing = await db.ConfiguracionSistema
            .FirstOrDefaultAsync(c => c.HotelId == null && c.Clave == clave, ct);

        if (existing is null)
        {
            db.ConfiguracionSistema.Add(new ConfiguracionSistema
            {
                Clave        = clave,
                Valor        = valor,
                TipoDato     = tipo,
                Descripcion  = descripcion,
                EsGlobal     = true,
                FechaCreacion = DateTime.UtcNow,
                ModificadoPor = modifiedBy
            });
        }
        else
        {
            existing.Valor            = valor;
            existing.FechaActualizacion = DateTime.UtcNow;
            existing.ModificadoPor    = modifiedBy;
        }
    }

    private static bool GetBool(Dictionary<string, string> map, string key, bool defaultValue = false)
        => map.TryGetValue(key, out var val) && bool.TryParse(val, out var b) ? b : defaultValue;

    private static int GetInt(Dictionary<string, string> map, string key, int defaultValue = 0)
        => map.TryGetValue(key, out var val) && int.TryParse(val, out var i) ? i : defaultValue;
}
