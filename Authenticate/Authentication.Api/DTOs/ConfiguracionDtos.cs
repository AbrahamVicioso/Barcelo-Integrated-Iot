namespace Authentication.Api.DTOs;

public record PasswordPolicyDto(
    int RequiredLength,
    bool RequireUppercase,
    bool RequireDigit,
    bool RequireNonAlphanumeric);

public record LockoutConfigDto(
    int MaxFailedAttempts,
    int LockoutMinutes);

public record SessionConfigDto(
    int TokenExpirationMinutes);

public record SignInConfigDto(
    bool RequireConfirmedEmail,
    bool AllowPasswordReset);

public record TwoFactorConfigDto(
    bool RequireForAdmins);

public record ConfiguracionIdentidadDto(
    PasswordPolicyDto Password,
    LockoutConfigDto Lockout,
    SessionConfigDto Session,
    SignInConfigDto SignIn,
    TwoFactorConfigDto TwoFactor);
