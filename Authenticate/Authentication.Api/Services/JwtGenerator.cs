using Authentication.Api.Contracts;
using Authentication.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;

namespace Authentication.Api.Services
{
    public class JwtGenerator : IJwtGenerator
    {
        private readonly IConfiguration configuration;

        public JwtGenerator(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task<string> GenerateJwtToken(IList<string> roles, User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            };

            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            var response = new JwtSecurityTokenHandler().WriteToken(token);

            if (response == null)
            {
                throw new Exception("Failed to generate JWT token.");
            }

            return response;
        }
    }
}
