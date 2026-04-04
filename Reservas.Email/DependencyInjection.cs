using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reservas.Application.Interfaces;
using Reservas.Email.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reservas.Email
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration["Email:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Missing required configuration 'Email:ConnectionString'. " +
                    "Set the environment variable 'Email__ConnectionString' with your Azure Communication Services connection string.");

            EmailClient emailClient = new EmailClient(connectionString);
            services.AddSingleton(emailClient);

            services.AddScoped<IEmailRepository, EmailRepository>();
            return services;
        }
    }
}
