using Authentication.Api.Data;
using Authentication.Domain.Entities;
using Barcelo.Authorization.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Authentication.Api
{
    public static class AuthorizationConfiguration
    {
        public static IServiceCollection AddAuthorizationConfiguration(this IServiceCollection services)
        {
            IConfiguration? configuration = services.BuildServiceProvider().GetService<IConfiguration>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(configuration["Jwt:Key"])
                    ),
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });

            services.AddIdentity<User, IdentityRole>(options =>
            {

                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedAccount = false;

                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<AuthenticationDbContext>()
            .AddApiEndpoints();

            services.AddBarceloAuthorization();

            // AddIdentity overrides DefaultAuthenticateScheme and DefaultChallengeScheme to
            // cookies, so JWT tokens are never evaluated. Re-set JWT as default after AddIdentity.
            services.Configure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            });

            // Add RoleManager explicitly
            services.AddScoped<RoleManager<IdentityRole>>();

            return services;
        }
    }
}
