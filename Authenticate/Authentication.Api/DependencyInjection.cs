using Authentication.Api.Contracts;
using Authentication.Api.Data;
using Authentication.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Api
{
    static class DependencyInjection
    {
        public static IServiceCollection AddServicesDependency(this IServiceCollection services)
        {
            services.AddDbContext<AuthenticationDbContext>(opt =>
            {
                opt.UseSqlServer("name=DefaultConnection");
            });

            services.AddScoped<IJwtGenerator, JwtGenerator>();

            return services;
        }
    }
}
