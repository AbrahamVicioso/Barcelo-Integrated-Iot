using Authentication.Domain.Entities;

namespace Authentication.Api.Contracts
{
    public interface IJwtGenerator
    {
        public Task<string> GenerateJwtToken(IList<String> roles, User user);
    }
}
