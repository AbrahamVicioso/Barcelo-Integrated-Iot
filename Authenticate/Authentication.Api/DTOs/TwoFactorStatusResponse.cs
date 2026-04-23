namespace Authentication.Api.DTOs
{
    public class TwoFactorStatusResponse
    {
        public bool IsTwoFactorEnabled { get; set; }
        public string TwoFactorProvider { get; set; } = string.Empty;
    }
}