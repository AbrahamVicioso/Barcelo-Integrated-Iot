using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification.Domain.Interfaces;
using Notification.Email;
using Notification.Kafka.Configuration;

namespace Notification.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IHostBuilder builder = Host.CreateDefaultBuilder(args);

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                if (context.HostingEnvironment.IsDevelopment())
                {
                    config.AddUserSecrets<Program>();
                }
                config.AddEnvironmentVariables();
            });

            builder.ConfigureServices((context, services) =>
            {
                // Configure SmtpSettings
                services.AddEmailService(context.Configuration);

                // Add Email Service
                services.AddSingleton<IEmailService, EmailService>();

                // Configure KafkaConsumerConfig
                var kafkaConfig = new KafkaConsumerConfig();
                context.Configuration.GetSection("KafkaConsumer").Bind(kafkaConfig);
                services.AddSingleton(kafkaConfig);

                // Add Kafka Consumer
                services.AddSingleton<IKafkaConsumer, Kafka.Services.NotificationKafkaConsumer>();

                // Add Background Service for Kafka Consumer
                services.AddHostedService<NotificationWorker>();
            });

            IHost host = builder.Build();

            await host.RunAsync();
        }
    }

    public class NotificationWorker : BackgroundService
    {
        private readonly IKafkaConsumer _kafkaConsumer;
        private readonly ILogger<NotificationWorker> _logger;

        public NotificationWorker(
            IKafkaConsumer kafkaConsumer,
            ILogger<NotificationWorker> logger)
        {
            _kafkaConsumer = kafkaConsumer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Notification Worker...");

            await _kafkaConsumer.StartAsync(stoppingToken);

            // Keep the worker running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Notification Worker...");

            await _kafkaConsumer.StopAsync(cancellationToken);

            await base.StopAsync(cancellationToken);
        }
    }
}
