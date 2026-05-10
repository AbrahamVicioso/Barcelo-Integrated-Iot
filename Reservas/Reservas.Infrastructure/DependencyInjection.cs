using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Notification.Domain.Interfaces;
using Reservas.Application.Interfaces;
using Reservas.Infrastructure.GrpcClients;
using Reservas.Infrastructure.Kafka;
using Reservas.Infrastructure.Services;

namespace Reservas.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
        {
            // Registrar Kafka Producer (reservas + unlock-door)
            var kafkaConfig = new KafkaProducerConfig();
            configuration.GetSection("KafkaProducer").Bind(kafkaConfig);
            services.AddSingleton(kafkaConfig);
            services.AddSingleton<IReservaKafkaProducer, ReservaKafkaProducer>();

            // Registrar Kafka Producer para actividades recreativas
            var actividadKafkaConfig = new ReservaActividadKafkaProducerConfig();
            configuration.GetSection("KafkaProducer").Bind(actividadKafkaConfig);
            actividadKafkaConfig.Topic = configuration["KafkaProducer:ActividadTopic"] ?? "actividades.reserva-confirmada";
            actividadKafkaConfig.ActividadUnlockDoorTopic = configuration["KafkaProducer:ActividadUnlockDoorTopic"] ?? "actividades.unlock-door";
            actividadKafkaConfig.PersonalActividadUnlockDoorTopic = configuration["KafkaProducer:PersonalActividadUnlockDoorTopic"] ?? "actividades.personal-unlock";
            actividadKafkaConfig.RecordatorioTopic = configuration["KafkaProducer:ActividadRecordatorioTopic"] ?? "actividades.recordatorio";
            actividadKafkaConfig.FechaActualizadaTopic = configuration["KafkaProducer:ActividadFechaActualizadaTopic"] ?? "actividades.fecha-actualizada";
            services.AddSingleton(actividadKafkaConfig);
            services.AddSingleton<IReservaActividadKafkaProducer, ReservaActividadKafkaProducer>();

            // Registrar hosted service de recordatorios de actividades
            services.AddHostedService<ActividadRecordatorioHostedService>();

            // Registrar Audit Kafka Producer
            var auditConfig = new AuditKafkaProducerConfig();
            configuration.GetSection("AuditProducer").Bind(auditConfig);
            services.AddSingleton(auditConfig);
            services.AddSingleton<IAuditProducer, AuditKafkaProducer>();

            // Registrar cliente gRPC → Usuarios.API
            var usuariosGrpcUrl = configuration["GrpcClients:Usuarios:GrpcUrl"]
                ?? configuration["Apis:Usuarios:BaseUrl"]  // clave legada, compatibilidad hacia atrás
                ?? throw new InvalidOperationException(
                    "Missing required configuration 'GrpcClients:Usuarios:GrpcUrl'. " +
                    "Add it to appsettings.json or set the environment variable " +
                    "'GrpcClients__Usuarios__GrpcUrl'.");

            var skipCertValidation = configuration.GetValue<bool>("GrpcClients:Usuarios:SkipCertValidation");

            services.AddSingleton<GrpcChannel>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<GrpcChannel>>();
                logger.LogInformation("Canal gRPC → Usuarios.API: {Url}", usuariosGrpcUrl);

                if (usuariosGrpcUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    // HTTPS: opcionalmente bypass de validación de cert (solo dev/Docker)
                    var httpHandler = new HttpClientHandler();
                    if (skipCertValidation)
                        httpHandler.ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                    return GrpcChannel.ForAddress(usuariosGrpcUrl.TrimEnd('/'), new GrpcChannelOptions
                    {
                        HttpHandler = httpHandler
                    });
                }

                // h2c (HTTP/2 cleartext) para desarrollo local
                var handler = new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true
                };
                var httpClient = new HttpClient(handler)
                {
                    DefaultRequestVersion = System.Net.HttpVersion.Version20,
                    DefaultVersionPolicy = System.Net.Http.HttpVersionPolicy.RequestVersionOrHigher
                };
                return GrpcChannel.ForAddress(usuariosGrpcUrl.TrimEnd('/'), new GrpcChannelOptions
                {
                    HttpClient = httpClient
                });
            });

            services.AddScoped<IUsuariosApiService>(sp =>
            {
                var channel = sp.GetRequiredService<GrpcChannel>();
                var logger = sp.GetRequiredService<ILogger<UsuariosGrpcClient>>();
                return new UsuariosGrpcClient(channel, logger);
            });

            return services;
        }
    }
}
