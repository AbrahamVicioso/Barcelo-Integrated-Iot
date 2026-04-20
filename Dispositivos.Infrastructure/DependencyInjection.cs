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

        // Singleton token cache shared across all TbDeviceService instances
        services.AddSingleton<TbTokenCache>();

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

        // Register Kafka consumer for check-in realizado events (auto-generates credentials)
        var checkInConfig = new CheckInRealizadoKafkaConsumerConfig();
        configuration.GetSection("KafkaConsumer:CheckInRealizado").Bind(checkInConfig);
        services.AddSingleton(checkInConfig);
        services.AddHostedService<CheckInRealizadoKafkaConsumer>();

        // Register Kafka consumer for personal unlock-door events
        var personalUnlockConfig = new PersonalUnlockDoorKafkaConsumerConfig();
        configuration.GetSection("KafkaConsumer:PersonalUnlockDoor").Bind(personalUnlockConfig);
        services.AddSingleton(personalUnlockConfig);
        services.AddHostedService<PersonalUnlockDoorKafkaConsumer>();

        // Register ThingsBoard credentials sync service (scoped — uses DbContext)
        services.AddScoped<ITbCredencialesSyncService, TbCredencialesSyncService>();

        // Register Kafka consumer for permiso personal creado events (triggers ThingsBoard sync)
        var permisoPersonalConfig = new PermisoPersonalCreadoKafkaConsumerConfig();
        configuration.GetSection("KafkaConsumer:PermisoPersonal").Bind(permisoPersonalConfig);
        services.AddSingleton(permisoPersonalConfig);
        services.AddHostedService<PermisoPersonalCreadoKafkaConsumer>();

        // Register Kafka consumer for cerradura acceso events (records access attempts from physical lock)
        var cerraduraAccesoConfig = new CerraduraAccesoKafkaConsumerConfig();
        configuration.GetSection("KafkaConsumer:CerraduraAcceso").Bind(cerraduraAccesoConfig);
        services.AddSingleton(cerraduraAccesoConfig);
        services.AddHostedService<CerraduraAccesoKafkaConsumer>();

        // Register Audit Kafka Producer
        var auditConfig = new AuditKafkaProducerConfig();
        configuration.GetSection("AuditProducer").Bind(auditConfig);
        services.AddSingleton(auditConfig);
        services.AddSingleton<IAuditProducer, AuditKafkaProducer>();

        return services;
    }
}
