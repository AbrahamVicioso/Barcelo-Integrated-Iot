using Authentication.Api.Contracts;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Barcelo.Authorization.Shared;

namespace Authentication.Api.Services;

public class JwtGenerator : IJwtGenerator
{
    private readonly IConfiguration configuration;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly IdentityRuntimeSettings runtimeSettings;

    public JwtGenerator(IConfiguration configuration, RoleManager<IdentityRole> roleManager, IdentityRuntimeSettings runtimeSettings)
    {
        this.configuration = configuration;
        this.roleManager = roleManager;
        this.runtimeSettings = runtimeSettings;
    }

    public async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(IList<string> roles, User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

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
                    claims.Add(new Claim(PermissionConstants.PermissionType, claim.Value));
            }
        }

        var accessToken = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(runtimeSettings.TokenExpirationMinutes),
            signingCredentials: creds
        ));

        var refreshExpiryDays = configuration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);
        var refreshClaims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(ClaimTypes.NameIdentifier, user.Id!),
        };

        var refreshToken = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: refreshClaims,
            expires: DateTime.UtcNow.AddDays(refreshExpiryDays),
            signingCredentials: creds
        ));

        return (accessToken, refreshToken);
    }

    public string? ValidateRefreshToken(string refreshToken)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal.FindFirstValue(ClaimTypes.NameIdentifier);
        }
        catch
        {
            return null;
        }
    }
}
