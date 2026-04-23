using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification.Domain.Interfaces;
using Notification.Email;
using Notification.Kafka.Configuration;
using Notification.Kafka.Services;
using Notification.Push;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Notification.Kafka.Data;

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
                // Add DbContext for NotificacionDbContext
                var connectionString = context.Configuration.GetConnectionString("BarceloIoTDatabase")
                    ?? throw new InvalidOperationException(
                        "Missing required configuration 'ConnectionStrings:BarceloIoTDatabase'. " +
                        "Add it to appsettings.json or set the environment variable 'ConnectionStrings__BarceloIoTDatabase'.");
                services.AddDbContext<NotificacionDbContext>(options =>
                    options.UseSqlServer(connectionString));

                // Add Repositories
                services.AddScoped<IPreferenciasRepository, PreferenciasRepository>();
                services.AddScoped<INotificacionesRepository, NotificacionesRepository>();

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

                // Configure PasswordResetConsumerConfig
                var passwordResetConsumerConfig = new PasswordResetConsumerConfig();
                context.Configuration.GetSection("KafkaConsumer:PasswordReset").Bind(passwordResetConsumerConfig);
                services.AddSingleton(passwordResetConsumerConfig);

                // Configure CredencialCreadaConsumerConfig
                var credencialCreadaConsumerConfig = new CredencialCreadaConsumerConfig();
                context.Configuration.GetSection("KafkaConsumer:CredencialCreada").Bind(credencialCreadaConsumerConfig);
                services.AddSingleton(credencialCreadaConsumerConfig);

                // Configure TwoFactorCodeConsumerConfig
                var twoFactorCodeConsumerConfig = new TwoFactorCodeConsumerConfig();
                context.Configuration.GetSection("KafkaConsumer:TwoFactorCode").Bind(twoFactorCodeConsumerConfig);
                services.AddSingleton(twoFactorCodeConsumerConfig);

                // Add Kafka Consumers as separate instances
                services.AddSingleton<UserCreatedEventConsumer>();
                services.AddSingleton<ReservaCreadaEventConsumer>();
                services.AddSingleton<EmailConfirmationEventConsumer>();
                services.AddSingleton<CredencialesCheckInEventConsumer>();
                services.AddSingleton<PersonalAccesoHabitacionEventConsumer>();
                services.AddSingleton<PasswordResetEventConsumer>();
                services.AddSingleton<CredencialCreadaEventConsumer>();
                services.AddSingleton<TwoFactorCodeEventConsumer>();

                // Add Background Services for Kafka Consumers
                services.AddHostedService<UserCreatedNotificationWorker>();
                services.AddHostedService<ReservaCreadaNotificationWorker>();
                services.AddHostedService<EmailConfirmationNotificationWorker>();
                services.AddHostedService<CredencialesCheckInNotificationWorker>();
                services.AddHostedService<PersonalAccesoHabitacionNotificationWorker>();
                services.AddHostedService<PasswordResetNotificationWorker>();
                services.AddHostedService<CredencialCreadaNotificationWorker>();
                services.AddHostedService<TwoFactorCodeNotificationWorker>();
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

    public class PasswordResetNotificationWorker : BackgroundService
    {
        private readonly PasswordResetEventConsumer _kafkaConsumer;
        private readonly ILogger<PasswordResetNotificationWorker> _logger;

        public PasswordResetNotificationWorker(
            PasswordResetEventConsumer kafkaConsumer,
            ILogger<PasswordResetNotificationWorker> logger)
        {
            _kafkaConsumer = kafkaConsumer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Password Reset Notification Worker...");
            await _kafkaConsumer.StartAsync(stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Password Reset Notification Worker...");
            await _kafkaConsumer.StopAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }

    public class CredencialCreadaNotificationWorker : BackgroundService
    {
        private readonly CredencialCreadaEventConsumer _kafkaConsumer;
        private readonly ILogger<CredencialCreadaNotificationWorker> _logger;

        public CredencialCreadaNotificationWorker(
            CredencialCreadaEventConsumer kafkaConsumer,
            ILogger<CredencialCreadaNotificationWorker> logger)
        {
            _kafkaConsumer = kafkaConsumer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Credencial Creada Notification Worker...");
            await _kafkaConsumer.StartAsync(stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Credencial Creada Notification Worker...");
            await _kafkaConsumer.StopAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }

    public class TwoFactorCodeNotificationWorker : BackgroundService
    {
        private readonly TwoFactorCodeEventConsumer _kafkaConsumer;
        private readonly ILogger<TwoFactorCodeNotificationWorker> _logger;

        public TwoFactorCodeNotificationWorker(
            TwoFactorCodeEventConsumer kafkaConsumer,
            ILogger<TwoFactorCodeNotificationWorker> logger)
        {
            _kafkaConsumer = kafkaConsumer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Two Factor Code Notification Worker...");
            await _kafkaConsumer.StartAsync(stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
                await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping Two Factor Code Notification Worker...");
            await _kafkaConsumer.StopAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
