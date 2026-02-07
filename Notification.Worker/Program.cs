using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification.Domain.Interfaces;
using Notification.Email;
using Notification.Kafka.Configuration;
using Notification.Kafka.Services;

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
                config.AddUserSecrets<Program>();
                config.AddEnvironmentVariables();
            });

            builder.ConfigureServices((context, services) =>
            {
                // Configure SmtpSettings
                services.AddEmailService(context.Configuration);

                // Add Email Service
                services.AddSingleton<IEmailService, EmailService>();

                // Configure UserCreatedConsumerConfig
                var userCreatedConsumerConfig = new UserCreatedConsumerConfig();
                context.Configuration.GetSection("KafkaConsumer:UserCreated").Bind(userCreatedConsumerConfig);
                services.AddSingleton(userCreatedConsumerConfig);

                // Configure ReservaCreadaConsumerConfig
                var reservaCreadaConsumerConfig = new ReservaCreadaConsumerConfig();
                context.Configuration.GetSection("KafkaConsumer:ReservaCreada").Bind(reservaCreadaConsumerConfig);
                services.AddSingleton(reservaCreadaConsumerConfig);

                // Add Kafka Consumers as separate instances
                services.AddSingleton<UserCreatedEventConsumer>();
                services.AddSingleton<ReservaCreadaEventConsumer>();

                // Add Background Services for Kafka Consumers
                services.AddHostedService<UserCreatedNotificationWorker>();
                services.AddHostedService<ReservaCreadaNotificationWorker>();
            });

            IHost host = builder.Build();

            await host.RunAsync();
        }
    }

    public class UserCreatedNotificationWorker : BackgroundService
    {
        private readonly UserCreatedEventConsumer _kafkaConsumer;
        private readonly ILogger<UserCreatedNotificationWorker> _logger;

        public UserCreatedNotificationWorker(
            UserCreatedEventConsumer kafkaConsumer,
            ILogger<UserCreatedNotificationWorker> logger)
        {
            _kafkaConsumer = kafkaConsumer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting User Created Notification Worker...");

            await _kafkaConsumer.StartAsync(stoppingToken);

            // Keep the worker running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping User Created Notification Worker...");

            await _kafkaConsumer.StopAsync(cancellationToken);

            await base.StopAsync(cancellationToken);
        }
    }

    public class ReservaCreadaNotificationWorker : BackgroundService
    {
        private readonly ReservaCreadaEventConsumer _kafkaConsumer;
        private readonly ILogger<ReservaCreadaNotificationWorker> _logger;

        public ReservaCreadaNotificationWorker(
            ReservaCreadaEventConsumer kafkaConsumer,
            ILogger<ReservaCreadaNotificationWorker> logger)
        {
            _kafkaConsumer = kafkaConsumer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Reserva Creada Notification Worker...");

            await _kafkaConsumer.StartAsync(stoppingToken);

            // Keep the worker running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Reserva Creada Notification Worker...");

            await _kafkaConsumer.StopAsync(cancellationToken);

            await base.StopAsync(cancellationToken);
        }
    }
}
