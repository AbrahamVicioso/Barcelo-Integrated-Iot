namespace Authentication.Api.DTOs
{
    public class TwoFactorLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}