using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using Notification.Domain.Entities;
using Notification.Domain.Events;
using Notification.Domain.Interfaces;
using Notification.Kafka.Configuration;

namespace Notification.Kafka.Services
{
    public class PasswordResetEventConsumer : NotificacionHandlerBase, IKafkaConsumer
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IAdminClient _adminClient;
        private readonly PasswordResetConsumerConfig _config;
        private readonly ILogger<PasswordResetEventConsumer> _logger;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _consumeTask;
        private bool _disposed;

        public bool IsRunning { get; private set; }

        public PasswordResetEventConsumer(
            PasswordResetConsumerConfig config,
            IPreferenciasRepository preferenciasRepo,
            INotificacionesRepository notificacionesRepo,
            AuthApiClient authApiClient,
            IEmailService emailService,
            IPushNotificationService pushService,
            ILogger<PasswordResetEventConsumer> logger)
            : base(preferenciasRepo, notificacionesRepo, authApiClient, emailService, pushService, logger)
        {
            _config = config;
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

            var adminConfig = new AdminClientConfig { BootstrapServers = config.BootstrapServers };
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

            _logger.LogInformation("PasswordResetEventConsumer started. Listening to topic: {Topic}", _config.Topic);

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
                _logger.LogInformation("PasswordResetEventConsumer stopping due to cancellation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in PasswordResetEventConsumer");
            }
        }

        private async Task ProcessMessageAsync(string messageValue, CancellationToken cancellationToken)
        {
            try
            {
                var resetEvent = JsonSerializer.Deserialize<PasswordResetEvent>(messageValue, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (resetEvent != null)
                {
                    await SendPasswordResetEmailAsync(resetEvent, cancellationToken);
                    return;
                }

                _logger.LogWarning("Could not deserialize message as PasswordResetEvent");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing message: {Message}", messageValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing password reset event");
            }
        }

        private async Task SendPasswordResetEmailAsync(PasswordResetEvent resetEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending password reset email to {Email}", resetEvent.Email);

            var emailBody = GeneratePasswordResetEmailBody(resetEvent);

            var emailNotification = new EmailNotification
            {
                To = resetEvent.Email,
                Subject = "Restablecer contraseña - Barcelo Integrated IoT",
                Body = emailBody,
                IsHtml = true
            };

            // Usar método de la clase base que verifica preferencias (solo email)
            await EnviarEmailAsync(
                resetEvent.Email,
                "RestablecerPassword",
                emailNotification,
                cancellationToken);
        }

        private string GeneratePasswordResetEmailBody(PasswordResetEvent resetEvent)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .btn {{ display: inline-block; padding: 14px 28px; background-color: #dc3545; color: white;
                text-decoration: none; border-radius: 4px; font-size: 16px; font-weight: bold; margin: 20px 0; }}
        .url-fallback {{ word-break: break-all; color: #555; font-size: 13px; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffc107; padding: 10px; border-radius: 4px; font-size: 13px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Restablecer contraseña</h1>
        </div>
        <div class='content'>
            <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta en <strong>Barcelo Integrated IoT</strong>.</p>
            <p>Haz clic en el botón para crear una nueva contraseña:</p>
            <p style='text-align:center;'>
                <a href='{resetEvent.ResetUrl}' class='btn'>Restablecer contraseña</a>
            </p>
            <p>Si el botón no funciona, copia y pega el siguiente enlace en tu navegador:</p>
            <p class='url-fallback'>{resetEvent.ResetUrl}</p>
            <div class='warning'>
                <strong>⚠️ Importante:</strong> Este enlace es de un solo uso y expira en 1 hora.
                Si no solicitaste este cambio, puedes ignorar este correo — tu contraseña no será modificada.
            </div>
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
                return Task.CompletedTask;

            _cancellationTokenSource?.Cancel();
            _consumeTask?.Wait(cancellationToken);

            _consumer.Close();
            IsRunning = false;

            _logger.LogInformation("PasswordResetEventConsumer stopped");

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _cancellationTokenSource?.Cancel();
            _consumer?.Close();
            _consumer?.Dispose();
            _adminClient?.Dispose();
            _cancellationTokenSource?.Dispose();

            _disposed = true;
        }
    }
}
