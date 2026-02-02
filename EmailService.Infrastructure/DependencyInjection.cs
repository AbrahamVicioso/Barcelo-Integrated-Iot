using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Notification.Domain.Interfaces;

namespace Notification.Email
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddEmailService(this IServiceCollection services, IConfiguration configuration)
        {
            var smtpSettings = new SmtpSettings();
            configuration.GetSection("SmtpSettings").Bind(smtpSettings);

            services.AddSingleton(smtpSettings);
            services.AddSingleton<IEmailService, EmailService>();

            return services;
        }
    }
}
