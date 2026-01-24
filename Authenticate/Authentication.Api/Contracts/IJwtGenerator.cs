using Authentication.Domain.Entities;
using System.Security.Cryptography;

namespace Authentication.Api.Contracts
{
    public interface IJwtGenerator
    {
        public Task<string> GenerateJwtToken(IList<String> roles, User user);
        public string GenerateRefreshToken();  
    }
}
