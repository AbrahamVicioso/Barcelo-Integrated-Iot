using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Usuarios.Domain.Interfaces;
using Microsoft.Extensions.Http;
using Usuarios.ExternalService.Repositories;

namespace Usuarios.ExternalService
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddExternalService(this IServiceCollection services)
        {
            services.AddHttpClient<IAuthenticationApiClient, AuthenticationApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:5019");
            });

            return services;
        }
    }
}
