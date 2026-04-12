using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification.Domain.Interfaces;
using Notification.Email;
using Notification.Kafka.Configuration;
using Notification.Kafka.Services;
using Notification.Push;
using Microsoft.Extensions.Logging;

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

                // Add Push Notification Service (ntfy)
                services.AddPushNotificationService(context.Configuration);

                // Register AuthApiClient (stores ntfy token in Authenticate.API)
                var authApiBaseUrl = context.Configuration["AuthApi:BaseUrl"]
                    ?? throw new InvalidOperationException(
                        "Missing required configuration 'AuthApi:BaseUrl'. " +
                        "Add it to appsettings.json or set the environment variable 'AuthApi__BaseUrl'.");
                services.AddHttpClient<AuthApiClient>(client =>
                {
                    client.BaseAddress = new Uri(authApiBaseUrl);
                });

                // Configure UserCreatedConsumerConfig
                var userCreatedConsumerConfig = new UserCreatedConsumerConfig();
                context.Configuration.GetSection("KafkaConsumer:UserCreated").Bind(userCreatedConsumerConfig);
                services.AddSingleton(userCreatedConsumerConfig);

                // Configure ReservaCreadaConsumerConfig
                var reservaCreadaConsumerConfig = new ReservaCreadaConsumerConfig();
                context.Configuration.GetSection("KafkaConsumer:ReservaCreada").Bind(reservaCreadaConsumerConfig);
                services.AddSingleton(reservaCreadaConsumerConfig);

                // Configure EmailConfirmationConsumerConfig
                var emailConfirmationConsumerConfig = new EmailConfirmationConsumerConfig();
                context.Configuration.GetSection("KafkaConsumer:EmailConfirmation").Bind(emailConfirmationConsumerConfig);
                services.AddSingleton(emailConfirmationConsumerConfig);

                // Configure CredencialesCheckInConsumerConfig
                var credencialesCheckInConsumerConfig = new CredencialesCheckInConsumerConfig();
                context.Configuration.GetSection("KafkaConsumer:CredencialesCheckIn").Bind(credencialesCheckInConsumerConfig);
                services.AddSingleton(credencialesCheckInConsumerConfig);

                // Configure PersonalAccesoHabitacionConsumerConfig
                var personalAccesoConsumerConfig = new PersonalAccesoHabitacionConsumerConfig();
                context.Configuration.GetSection("KafkaConsumer:PersonalAccesoHabitacion").Bind(personalAccesoConsumerConfig);
                services.AddSingleton(personalAccesoConsumerConfig);

                // Add Kafka Consumers as separate instances
                services.AddSingleton<UserCreatedEventConsumer>();
                services.AddSingleton<ReservaCreadaEventConsumer>();
                services.AddSingleton<EmailConfirmationEventConsumer>();
                services.AddSingleton<CredencialesCheckInEventConsumer>();
                services.AddSingleton<PersonalAccesoHabitacionEventConsumer>();

                // Add Background Services for Kafka Consumers
                services.AddHostedService<UserCreatedNotificationWorker>();
                services.AddHostedService<ReservaCreadaNotificationWorker>();
                services.AddHostedService<EmailConfirmationNotificationWorker>();
                services.AddHostedService<CredencialesCheckInNotificationWorker>();
                services.AddHostedService<PersonalAccesoHabitacionNotificationWorker>();
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

    public class EmailConfirmationNotificationWorker : BackgroundService
    {
        private readonly EmailConfirmationEventConsumer _kafkaConsumer;
        private readonly ILogger<EmailConfirmationNotificationWorker> _logger;

        public EmailConfirmationNotificationWorker(
            EmailConfirmationEventConsumer kafkaConsumer,
            ILogger<EmailConfirmationNotificationWorker> logger)
        {
            _kafkaConsumer = kafkaConsumer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Email Confirmation Notification Worker...");
            await _kafkaConsumer.StartAsync(stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Email Confirmation Notification Worker...");
            await _kafkaConsumer.StopAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }

    public class CredencialesCheckInNotificationWorker : BackgroundService
    {
        private readonly CredencialesCheckInEventConsumer _kafkaConsumer;
        private readonly ILogger<CredencialesCheckInNotificationWorker> _logger;

        public CredencialesCheckInNotificationWorker(
            CredencialesCheckInEventConsumer kafkaConsumer,
            ILogger<CredencialesCheckInNotificationWorker> logger)
        {
            _kafkaConsumer = kafkaConsumer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Credenciales CheckIn Notification Worker...");
            await _kafkaConsumer.StartAsync(stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Credenciales CheckIn Notification Worker...");
            await _kafkaConsumer.StopAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }

    public class PersonalAccesoHabitacionNotificationWorker : BackgroundService
    {
        private readonly PersonalAccesoHabitacionEventConsumer _kafkaConsumer;
        private readonly ILogger<PersonalAccesoHabitacionNotificationWorker> _logger;

        public PersonalAccesoHabitacionNotificationWorker(
            PersonalAccesoHabitacionEventConsumer kafkaConsumer,
            ILogger<PersonalAccesoHabitacionNotificationWorker> logger)
        {
            _kafkaConsumer = kafkaConsumer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Personal Acceso Habitacion Notification Worker...");
            await _kafkaConsumer.StartAsync(stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Personal Acceso Habitacion Notification Worker...");
            await _kafkaConsumer.StopAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
