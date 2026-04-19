using Authentication.Api.Contracts;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Barcelo.Authorization.Shared;

namespace Authentication.Api.Services;

public class JwtGenerator : IJwtGenerator
{
    private readonly IConfiguration configuration;
    private readonly RoleManager<IdentityRole> roleManager;

    public JwtGenerator(IConfiguration configuration, RoleManager<IdentityRole> roleManager)
    {
        this.configuration = configuration;
        this.roleManager = roleManager;
    }

    public async Task<string> GenerateJwtToken(IList<string> roles, User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(ClaimTypes.NameIdentifier, user.Id!),
        };

        foreach (string role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));

            var identityRole = await roleManager.FindByNameAsync(role);
            if (identityRole != null)
            {
                var roleClaims = await roleManager.GetClaimsAsync(identityRole);
                foreach (var claim in roleClaims)
                {
                    claims.Add(new Claim(PermissionConstants.PermissionType, claim.Value));
                }
            }
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

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}