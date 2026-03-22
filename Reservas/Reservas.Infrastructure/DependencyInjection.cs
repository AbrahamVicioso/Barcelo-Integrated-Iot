using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Notification.Domain.Interfaces;
using Reservas.Application.Interfaces;
using Reservas.Infrastructure.GrpcClients;
using Reservas.Infrastructure.Kafka;

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

            // Registrar Audit Kafka Producer
            var auditConfig = new AuditKafkaProducerConfig();
            configuration.GetSection("AuditProducer").Bind(auditConfig);
            services.AddSingleton(auditConfig);
            services.AddSingleton<IAuditProducer, AuditKafkaProducer>();

            // Registrar cliente gRPC → Usuarios.API
            var usuariosGrpcUrl = configuration["GrpcClients:Usuarios:GrpcUrl"]
                ?? configuration["Apis:Usuarios:BaseUrl"]
                ?? "http://localhost:5284";

            services.AddSingleton<GrpcChannel>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<GrpcChannel>>();
                logger.LogInformation("Canal gRPC → Usuarios.API: {Url}", usuariosGrpcUrl);

                var handler = new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true
                };

                // Forzar HTTP/2 en el cliente para h2c (sin TLS)
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
