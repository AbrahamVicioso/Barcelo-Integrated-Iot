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
            EmailClient emailClient = new EmailClient(configuration.GetSection("Email:ConnectionString").Value);
            services.AddSingleton(emailClient); 

            services.AddScoped<IEmailRepository, EmailRepository>();
            return services;
        }
    }
}
