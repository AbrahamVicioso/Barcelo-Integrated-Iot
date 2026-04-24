using Authentication.Api.DTOs;

namespace Authentication.Api.Services;

public interface IIdentityConfiguracionService
{
    Task<ConfiguracionIdentidadDto> GetAllAsync(CancellationToken ct = default);
    Task UpdatePasswordPolicyAsync(PasswordPolicyDto dto, string modifiedBy, CancellationToken ct = default);
    Task UpdateLockoutAsync(LockoutConfigDto dto, string modifiedBy, CancellationToken ct = default);
    Task UpdateSessionAsync(SessionConfigDto dto, string modifiedBy, CancellationToken ct = default);
    Task UpdateSignInAsync(SignInConfigDto dto, string modifiedBy, CancellationToken ct = default);
    Task UpdateTwoFactorAsync(TwoFactorConfigDto dto, string modifiedBy, CancellationToken ct = default);
    Task ApplyAllToIdentityAsync(CancellationToken ct = default);
}
