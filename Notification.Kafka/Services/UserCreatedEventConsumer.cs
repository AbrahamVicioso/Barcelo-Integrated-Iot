using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using Notification.Domain.Entities;
using Notification.Domain.Events;
using Notification.Domain.Helpers;
using Notification.Domain.Interfaces;
using Notification.Kafka.Configuration;

namespace Notification.Kafka.Services
{
    public class UserCreatedEventConsumer : NotificacionHandlerBase, IKafkaConsumer
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IAdminClient _adminClient;
        private readonly INtfyAdminService _ntfyAdmin;
        private readonly AuthApiClient _authApiClient;
        private readonly UserCreatedConsumerConfig _config;
        private readonly ILogger<UserCreatedEventConsumer> _logger;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _consumeTask;
        private bool _disposed;

        public bool IsRunning { get; private set; }

        public UserCreatedEventConsumer(
            UserCreatedConsumerConfig config,
            IPreferenciasRepository preferenciasRepo,
            INotificacionesRepository notificacionesRepo,
            AuthApiClient authApiClient,
            IEmailService emailService,
            IPushNotificationService pushService,
            INtfyAdminService ntfyAdmin,
            ILogger<UserCreatedEventConsumer> logger)
            : base(preferenciasRepo, notificacionesRepo, authApiClient, emailService, pushService, logger)
        {
            _config = config;
            _ntfyAdmin = ntfyAdmin;
            _authApiClient = authApiClient;
            _logger = logger;

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config.BootstrapServers,
                GroupId = config.GroupId,
                AutoOffsetReset = Enum.Parse<AutoOffsetReset>(config.AutoOffsetReset, true),
                EnableAutoCommit = config.EnableAutoCommit,
                AutoCommitIntervalMs = config.AutoCommitIntervalMs,
                SessionTimeoutMs = config.SessionTimeoutMs,
                MaxPollIntervalMs = config.MaxPollIntervalMs
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig)
                .SetErrorHandler((_, e) => _logger.LogError("Kafka error: {Reason}", e.Reason))
                .SetPartitionsAssignedHandler((c, partitions) =>
                {
                    _logger.LogInformation("Assigned partitions: {Partitions}", string.Join(", ", partitions));
                })
                .Build();

            var adminConfig = new AdminClientConfig
            {
                BootstrapServers = config.BootstrapServers
            };
            _adminClient = new AdminClientBuilder(adminConfig).Build();
        }

        private void EnsureTopicExists()
        {
            try
            {
                var metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(10));
                var topics = metadata.Topics.Select(t => t.Topic).ToList();

                if (!topics.Contains(_config.Topic))
                {
                    _logger.LogInformation("Topic {Topic} does not exist. Creating...", _config.Topic);
                    
                    var topicSpec = new TopicSpecification
                    {
                        Name = _config.Topic,
                        NumPartitions = 1,
                        ReplicationFactor = 1
                    };

                    _adminClient.CreateTopicsAsync(new List<TopicSpecification> { topicSpec }).GetAwaiter().GetResult();
                    _logger.LogInformation("Topic {Topic} created successfully", _config.Topic);
                }
                else
                {
                    _logger.LogInformation("Topic {Topic} already exists", _config.Topic);
                }
            }
            catch (CreateTopicsException ex) when (ex.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
            {
                _logger.LogInformation("Topic {Topic} already exists (handled)", _config.Topic);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error ensuring topic exists. Consumer will attempt to subscribe anyway.");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (IsRunning)
            {
                _logger.LogWarning("Consumer is already running");
                return Task.CompletedTask;
            }

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            EnsureTopicExists();
            
            _consumer.Subscribe(_config.Topic);
            IsRunning = true;

            _consumeTask = Task.Run(() => ConsumeMessages(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

            _logger.LogInformation("UserCreatedEventConsumer started. Listening to topic: {Topic}", _config.Topic);

            return Task.CompletedTask;
        }

        private async Task ConsumeMessages(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(cancellationToken);

                        if (consumeResult != null)
                        {
                            await ProcessMessageAsync(consumeResult.Message.Value, cancellationToken);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming message from Kafka");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("UserCreatedEventConsumer stopping due to cancellation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in UserCreatedEventConsumer");
            }
        }

        private async Task ProcessMessageAsync(string messageValue, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Received message: {Message}", messageValue);

                var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(messageValue, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (userCreatedEvent != null)
                {
                    await ProcessUserCreatedEventAsync(userCreatedEvent, cancellationToken);
                    return;
                }

                _logger.LogWarning("Could not deserialize message as UserCreatedEvent");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing message: {Message}", messageValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing user created event");
            }
        }

        private async Task ProcessUserCreatedEventAsync(UserCreatedEvent userEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing UserCreatedEvent for user: {Email}", userEvent.Email);

            // 1. Create ntfy account → get access token (password is discarded internally)
            var ntfyToken = await _ntfyAdmin.CreateUserAccountAsync(userEvent.Email, cancellationToken);
            var ntfyTopic = NtfyTopicHelper.GetUserTopic(userEvent.Email);

            // 2. Store the token in Authenticate.API so users can retrieve it via JWT-auth endpoint
            if (ntfyToken != null)
                await _authApiClient.StoreNtfyTokenAsync(userEvent.Email, ntfyToken, cancellationToken);

            // 3. Send welcome email (no ntfy credentials — user fetches them via the API)
            var emailBody = GenerateUserCreatedEmailBody(userEvent);

            var emailNotification = new EmailNotification
            {
                To = userEvent.Email,
                Subject = "Bienvenido a Barcelo Integrated IoT - Tu cuenta ha sido creada",
                Body = emailBody,
                IsHtml = true
            };

            var pushNotification = new PushNotification
            {
                Topic = ntfyTopic,
                Title = "Bienvenido a Barcelo IoT",
                Message = $"Hola {userEvent.UserName}, tu cuenta ha sido creada exitosamente.",
                Priority = PushPriority.Default,
                Tags = ["wave", "hotel"]
            };

            // Usar métodos de la clase base que verifican preferencias
            await EnviarNotificacionCompletaAsync(
                userEvent.Email,
                "CuentaCreada",
                emailNotification,
                pushNotification,
                cancellationToken);
        }

        private string GenerateUserCreatedEmailBody(UserCreatedEvent userEvent)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .credentials {{ background-color: #fff; border: 1px solid #ddd; padding: 15px; margin: 15px 0; }}
        .password {{ font-family: monospace; font-size: 18px; color: #007bff; font-weight: bold; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Bienvenido a Barcelo Integrated IoT</h1>
        </div>
        <div class='content'>
            <p>Hola <strong>{userEvent.UserName}</strong>,</p>
            <p>Tu cuenta ha sido creada exitosamente en el sistema Barcelo Integrated IoT.</p>
            <div class='credentials'>
                <p><strong>Credenciales de acceso al sistema:</strong></p>
                <p>Email: {userEvent.Email}</p>
                <p>Contraseña: <span class='password'>{userEvent.GeneratedPassword}</span></p>
            </div>
            <p>Para activar las notificaciones push, inicia sesión en la app y accede a <strong>Configuración → Notificaciones</strong>. Tu token de acceso a ntfy se genera automáticamente.</p>
            <p>Por seguridad, te recomendamos cambiar tu contraseña después de iniciar sesión por primera vez.</p>
            <p>Si tienes alguna pregunta, no dudes en contactar a nuestro equipo de soporte.</p>
        </div>
        <div class='footer'>
            <p>© 2026 Barcelo Integrated IoT. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (!IsRunning)
            {
                return Task.CompletedTask;
            }

            _cancellationTokenSource?.Cancel();
            _consumeTask?.Wait(cancellationToken);

            _consumer.Close();
            IsRunning = false;

            _logger.LogInformation("UserCreatedEventConsumer stopped");

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _cancellationTokenSource?.Cancel();
            _consumer?.Close();
            _consumer?.Dispose();
            _adminClient?.Dispose();
            _cancellationTokenSource?.Dispose();

            _disposed = true;
        }
    }
}
