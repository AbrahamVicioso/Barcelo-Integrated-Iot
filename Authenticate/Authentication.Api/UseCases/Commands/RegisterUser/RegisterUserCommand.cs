namespace Authentication.Api.UseCases.Commands.RegisterUser
{
    public class RegisterUserCommand
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
