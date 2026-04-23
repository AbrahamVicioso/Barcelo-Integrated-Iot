namespace Authentication.Api.DTOs;

public class TwoFactorConfirmRequest
{
    public string Code { get; set; } = string.Empty;
}