using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Notification.Domain.Interfaces;
using Azure.Communication.Email;

namespace Notification.Email
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddEmailService(this IServiceCollection services, IConfiguration configuration)
        {
            var smtpSettings = new SmtpSettings();
            configuration.GetSection("EmailSettings").Bind(smtpSettings);

            var emailClient = new EmailClient(smtpSettings.ConnectionString);

            services.AddSingleton(emailClient);

            services.AddSingleton(smtpSettings);
            services.AddSingleton<IEmailService, EmailService>();

            return services;
        }
    }
}
