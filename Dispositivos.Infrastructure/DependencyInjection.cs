using Dispositivos.Application.Interfaces;
using Dispositivos.Infrastructure.Configuration;
using Dispositivos.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Notification.Domain.Interfaces;

namespace Dispositivos.Infrastructure;

/// <summary>
/// Dependency injection extensions for Thingsboard infrastructure
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Thingsboard infrastructure services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddThingsboardInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure Thingsboard options
        services.Configure<ThingsboardOptions>(
            configuration.GetSection("Thingsboard"));

        // Register HttpClient for Thingsboard using factory pattern
        services.AddHttpClient<ITbDeviceService, TbDeviceService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<ThingsboardOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // Register Kafka consumer for unlock-door events
        var unlockDoorConfig = new UnlockDoorKafkaConsumerConfig();
        configuration.GetSection("KafkaConsumer:UnlockDoor").Bind(unlockDoorConfig);
        services.AddSingleton(unlockDoorConfig);
        services.AddHostedService<UnlockDoorKafkaConsumer>();

        // Register Audit Kafka Producer
        var auditConfig = new AuditKafkaProducerConfig();
        configuration.GetSection("AuditProducer").Bind(auditConfig);
        services.AddSingleton(auditConfig);
        services.AddSingleton<IAuditProducer, AuditKafkaProducer>();

        return services;
    }
}
