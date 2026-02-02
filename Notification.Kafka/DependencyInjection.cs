using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Domain.Interfaces;
using Notification.Kafka.Configuration;
using Notification.Kafka.Services;

namespace Notification.Kafka
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddNotificationKafkaConsumer(this IServiceCollection services, IConfiguration configuration)
        {
            var kafkaConfig = new KafkaConsumerConfig();
            configuration.GetSection("KafkaConsumer").Bind(kafkaConfig);

            services.AddSingleton(kafkaConfig);
            services.AddSingleton<IKafkaConsumer, NotificationKafkaConsumer>();

            return services;
        }
    }
}
