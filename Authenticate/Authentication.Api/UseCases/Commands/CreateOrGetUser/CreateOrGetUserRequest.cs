using System.ComponentModel.DataAnnotations;

namespace Authentication.Api.UseCases.Commands.CreateOrGetUser
{
    public class CreateOrGetUserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
