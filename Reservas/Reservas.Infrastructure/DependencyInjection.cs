using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reservas.Application.Interfaces;
using Reservas.Infrastructure.Kafka;

namespace Reservas.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
        {
            // Register Kafka Producer
            var kafkaConfig = new KafkaProducerConfig();
            configuration.GetSection("KafkaProducer").Bind(kafkaConfig);
            services.AddSingleton(kafkaConfig);
            services.AddSingleton<IReservaKafkaProducer, ReservaKafkaProducer>();

            return services;
        }
    }
}
