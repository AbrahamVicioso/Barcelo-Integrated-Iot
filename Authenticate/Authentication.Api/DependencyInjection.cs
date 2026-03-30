using Authentication.Api.Contracts;
using Authentication.Api.Data;
using Authentication.Api.Services;
using Authentication.Api.UseCases.Commands.LoginUser;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Notification.Domain.Interfaces;
using Notification.Push;

namespace Authentication.Api
{
    static class DependencyInjection
    {
        public static IServiceCollection AddServicesDependency(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AuthenticationDbContext>(opt =>
            {
                opt.UseSqlServer("name=DefaultConnection");
            });

            services.AddScoped<IJwtGenerator, JwtGenerator>();

            // Configure Kafka Producer (user-created events)
            var kafkaConfig = new KafkaProducerConfig();
            configuration.GetSection("KafkaProducer").Bind(kafkaConfig);
            services.AddSingleton(kafkaConfig);
            services.AddSingleton<IKafkaProducerService, KafkaProducerService>();

            // Register Audit Kafka Producer
            var auditConfig = new AuditKafkaProducerConfig();
            configuration.GetSection("AuditProducer").Bind(auditConfig);
            services.AddSingleton(auditConfig);
            services.AddSingleton<IAuditProducer, AuditKafkaProducer>();

            // Register ntfy admin service (used by NotificationsController to grant system topic access)
            var ntfyOptions = new NtfyOptions();
            configuration.GetSection("Ntfy").Bind(ntfyOptions);
            services.AddSingleton(ntfyOptions);
            services.AddHttpClient<INtfyAdminService, NtfyAdminService>()
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler();
                    if (ntfyOptions.IgnoreCertificateErrors)
                        handler.ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    return handler;
                });

            return services;
        }
    }
}
