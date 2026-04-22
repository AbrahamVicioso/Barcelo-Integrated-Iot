using Authentication.Domain.Entities;

namespace Authentication.Api.Contracts
{
    public interface IJwtGenerator
    {
        public Task<(string accessToken, string refreshToken)> GenerateTokensAsync(IList<string> roles, User user);
        public string? ValidateRefreshToken(string refreshToken);
    }
}
