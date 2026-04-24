namespace Authentication.Api.Services;

public class IdentityRuntimeSettings
{
    public int TokenExpirationMinutes { get; set; } = 30;
    public bool TwoFactorRequireForAdmins { get; set; } = false;
    public bool AllowPasswordReset { get; set; } = true;
}
