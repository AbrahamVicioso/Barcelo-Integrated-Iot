using Dispositivos.Application.Interfaces;
using Dispositivos.Infrastructure.Configuration;
using Dispositivos.Infrastructure.GrpcClients;
using Dispositivos.Infrastructure.Services;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        // Register Kafka consumer for reserva actividad confirmada events (generates activity lock credentials)
        var reservaActividadConfig = new ReservaActividadKafkaConsumerConfig();
        configuration.GetSection("KafkaConsumer:ReservaActividad").Bind(reservaActividadConfig);
        services.AddSingleton(reservaActividadConfig);
        services.AddHostedService<ReservaActividadKafkaConsumer>();

        // Register Kafka consumer for reserva huesped actualizado events (creates/deactivates credentials)
        var huespedActualizadoConfig = new ReservaHuespedActualizadoKafkaConsumerConfig();
        configuration.GetSection("KafkaConsumer:HuespedActualizado").Bind(huespedActualizadoConfig);
        services.AddSingleton(huespedActualizadoConfig);
        services.AddHostedService<ReservaHuespedActualizadoKafkaConsumer>();

        // Register Kafka consumer for actividad fecha actualizada events (updates credential dates)
        var actividadFechaConfig = new ActividadFechaActualizadaKafkaConsumerConfig();
        configuration.GetSection("KafkaConsumer:ActividadFechaActualizada").Bind(actividadFechaConfig);
        services.AddSingleton(actividadFechaConfig);
        services.AddHostedService<ActividadFechaActualizadaKafkaConsumer>();

        // Register Kafka consumer for actividad unlock-door events (huesped)
        var actividadUnlockConfig = new ActividadUnlockDoorKafkaConsumerConfig();
        configuration.GetSection("KafkaConsumer:ActividadUnlockDoor").Bind(actividadUnlockConfig);
        services.AddSingleton(actividadUnlockConfig);
        services.AddHostedService<ActividadUnlockDoorKafkaConsumer>();

        // Register Kafka consumer for personal actividad unlock-door events
        var personalActividadUnlockConfig = new PersonalActividadUnlockDoorKafkaConsumerConfig();
        configuration.GetSection("KafkaConsumer:PersonalActividadUnlockDoor").Bind(personalActividadUnlockConfig);
        services.AddSingleton(personalActividadUnlockConfig);
        services.AddHostedService<PersonalActividadUnlockDoorKafkaConsumer>();

        // Register gRPC client → Usuarios.API
        var usuariosGrpcUrl = configuration["ExternalServices:Usuarios:GrpcUrl"]
            ?? throw new InvalidOperationException("Missing 'ExternalServices:Usuarios:GrpcUrl' configuration.");
        var skipUsuariosCert = configuration.GetValue<bool>("ExternalServices:Usuarios:SkipCertValidation");

        services.AddSingleton<GrpcChannel>(sp =>
        {
            if (usuariosGrpcUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                var httpHandler = new HttpClientHandler();
                if (skipUsuariosCert)
                    httpHandler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                return GrpcChannel.ForAddress(usuariosGrpcUrl.TrimEnd('/'), new GrpcChannelOptions { HttpHandler = httpHandler });
            }

            var handler = new SocketsHttpHandler { EnableMultipleHttp2Connections = true };
            var httpClient = new HttpClient(handler)
            {
                DefaultRequestVersion = System.Net.HttpVersion.Version20,
                DefaultVersionPolicy = System.Net.Http.HttpVersionPolicy.RequestVersionOrHigher
            };
            return GrpcChannel.ForAddress(usuariosGrpcUrl.TrimEnd('/'), new GrpcChannelOptions { HttpClient = httpClient });
        });

        services.AddScoped<IUsuariosGrpcService>(sp =>
        {
            var channel = sp.GetRequiredService<GrpcChannel>();
            var logger = sp.GetRequiredService<ILogger<UsuariosGrpcClient>>();
            return new UsuariosGrpcClient(channel, logger);
        });

        // Register Audit Kafka Producer
        var auditConfig = new AuditKafkaProducerConfig();
        configuration.GetSection("AuditProducer").Bind(auditConfig);
        services.AddSingleton(auditConfig);
        services.AddSingleton<IAuditProducer, AuditKafkaProducer>();

        // Register Credentials Kafka Producer (for sending email/push on credential creation)
        var credencialesProducerConfig = new CredencialesKafkaProducerConfig();
        configuration.GetSection("KafkaProducer").Bind(credencialesProducerConfig);
        services.AddSingleton(credencialesProducerConfig);
        services.AddSingleton<ICredencialesKafkaProducer, CredencialesKafkaProducer>();

        return services;
    }
}
