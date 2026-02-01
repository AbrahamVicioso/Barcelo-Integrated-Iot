using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Usuarios.Domain.Interfaces;
using Microsoft.Extensions.Http;
using Usuarios.ExternalService.Repositories;
using Usuarios.ExternalService.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Usuarios.ExternalService
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddExternalService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AuthenticationApiOptions>(configuration.GetSection(AuthenticationApiOptions.SectionName));

            services.AddHttpClient<IAuthenticationApiClient, AuthenticationApiClient>((sp,client) =>
            {
                var options = sp.GetRequiredService<IOptions<AuthenticationApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl );
            });

            return services;
        }
    }
}
