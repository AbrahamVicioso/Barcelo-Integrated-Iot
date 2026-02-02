namespace Authentication.Api.DTOs
{
    public class CreateUserWithRandomPasswordResponse
    {
        public string Email { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
