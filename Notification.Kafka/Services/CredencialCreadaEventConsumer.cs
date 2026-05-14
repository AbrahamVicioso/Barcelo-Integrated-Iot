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
    public class CredencialCreadaEventConsumer : NotificacionHandlerBase, IKafkaConsumer
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IAdminClient _adminClient;
        private readonly CredencialCreadaConsumerConfig _config;
        private readonly ILogger<CredencialCreadaEventConsumer> _logger;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _consumeTask;
        private bool _disposed;

        public bool IsRunning { get; private set; }

        public CredencialCreadaEventConsumer(
            CredencialCreadaConsumerConfig config,
            IPreferenciasRepository preferenciasRepo,
            INotificacionesRepository notificacionesRepo,
            AuthApiClient authApiClient,
            IEmailService emailService,
            IPushNotificationService pushService,
            ILogger<CredencialCreadaEventConsumer> logger)
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
                .SetPartitionsAssignedHandler((_, partitions) =>
                    _logger.LogInformation("Assigned partitions: {Partitions}", string.Join(", ", partitions)))
                .Build();

            _adminClient = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = config.BootstrapServers
            }).Build();
        }

        private void EnsureTopicExists()
        {
            try
            {
                var metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(10));
                if (!metadata.Topics.Any(t => t.Topic == _config.Topic))
                {
                    _adminClient.CreateTopicsAsync(new[]
                    {
                        new TopicSpecification { Name = _config.Topic, NumPartitions = 1, ReplicationFactor = 1 }
                    }).GetAwaiter().GetResult();
                    _logger.LogInformation("Topic {Topic} created", _config.Topic);
                }
            }
            catch (CreateTopicsException ex) when (ex.Results[0].Error.Code == ErrorCode.TopicAlreadyExists) { }
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

            _logger.LogInformation("CredencialCreadaEventConsumer started. Listening to topic: {Topic}", _config.Topic);
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
                            await ProcessMessageAsync(consumeResult.Message.Value, cancellationToken);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming message from Kafka");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("CredencialCreadaEventConsumer stopping due to cancellation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in CredencialCreadaEventConsumer");
            }
        }

        private async Task ProcessMessageAsync(string messageValue, CancellationToken cancellationToken)
        {
            try
            {
                var credencialEvent = JsonSerializer.Deserialize<CredencialCreadaEvent>(messageValue,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (credencialEvent == null)
                {
                    _logger.LogWarning("Could not deserialize CredencialCreadaEvent");
                    return;
                }

                await ProcessCredencialCreadaEventAsync(credencialEvent, cancellationToken);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing CredencialCreadaEvent: {Message}", messageValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing CredencialCreadaEvent");
            }
        }

        private async Task ProcessCredencialCreadaEventAsync(CredencialCreadaEvent credencialEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Procesando CredencialCreadaEvent para credencial {CredencialId}, tipo: {TipoCredencial}",
                credencialEvent.CredencialId, credencialEvent.TipoCredencial);

            if (string.IsNullOrEmpty(credencialEvent.Email))
            {
                _logger.LogWarning("Credencial {CredencialId} no tiene email asociado, omitiendo notificación", credencialEvent.CredencialId);
                return;
            }

            try
            {
                var tipoTexto = credencialEvent.TipoCredencial switch {
                    "Huesped" => "de huésped",
                    "Actividad" => "de actividad",
                    _ => "de personal"
                };
                var emailBody = GenerarEmailCredencial(credencialEvent);

                var emailNotification = new EmailNotification
                {
                    To = credencialEvent.Email,
                    Subject = $"Tu credencial de acceso - {credencialEvent.TipoCredencial}",
                    Body = emailBody,
                    IsHtml = true
                };

                var pushNotification = new PushNotification
                {
                    Topic = NtfyTopicHelper.GetUserTopic(credencialEvent.Email),
                    Title = "Nueva credencial de acceso creada",
                    Message = $"Tu credencial {tipoTexto} ha sido creada. PIN: {credencialEvent.CodigoPin}. Válido hasta {credencialEvent.FechaExpiracion:dd/MM/yyyy HH:mm}.",
                    Priority = PushPriority.High,
                    Tags = ["key", "hotel"]
                };

                // Usar métodos de la clase base que verifican preferencias
                await EnviarNotificacionCompletaAsync(
                    credencialEvent.Email,
                    "Credencial",
                    emailNotification,
                    pushNotification,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error enviando notificación para credencial {CredencialId}",
                    credencialEvent.CredencialId);
            }
        }

        private string GenerarEmailCredencial(CredencialCreadaEvent credencialEvent)
        {
            var tipoTexto = credencialEvent.TipoCredencial switch {
                "Huesped" => "huésped",
                "Actividad" => "actividad",
                _ => "personal"
            };
            var nombre = credencialEvent.NombreCompleto ?? "Usuario";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #1a73e8; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .pin-box {{ background-color: #fff; border: 2px solid #1a73e8; border-radius: 8px; padding: 20px; margin: 20px 0; text-align: center; }}
        .pin {{ font-family: monospace; font-size: 36px; color: #1a73e8; font-weight: bold; letter-spacing: 8px; }}
        .details {{ background-color: #fff; border: 1px solid #ddd; padding: 15px; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffc107; padding: 10px; border-radius: 4px; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Tu credencial de acceso</h1>
        </div>
        <div class='content'>
            <p>Hola <strong>{nombre}</strong>,</p>
            <p>Se ha creado una nueva credencial de acceso como {tipoTexto}. A continuación encontrarás los detalles:</p>
            <div class='pin-box'>
                <p style='margin: 0; font-size: 14px; color: #666;'>TU PIN DE ACCESO</p>
                <div class='pin'>{credencialEvent.CodigoPin}</div>
            </div>
            <div class='details'>
                <p><strong>Tipo de credencial:</strong> {credencialEvent.TipoCredencial}</p>
                <p><strong>Fecha de activación:</strong> {credencialEvent.FechaActivacion:dd/MM/yyyy HH:mm}</p>
                <p><strong>Fecha de expiración:</strong> {credencialEvent.FechaExpiracion:dd/MM/yyyy HH:mm}</p>
            </div>
            <div class='warning'>
                <strong>⚠️ Importante:</strong> No compartas tu PIN con nadie. Este código es personal e intransferible.
            </div>
            <p>Para abrir la puerta, usa el PIN desde la app o ingresa el PIN directamente en la cerradura inteligente.</p>
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
            if (!IsRunning) return Task.CompletedTask;

            _cancellationTokenSource?.Cancel();
            _consumeTask?.Wait(cancellationToken);
            _consumer.Close();
            IsRunning = false;

            _logger.LogInformation("CredencialCreadaEventConsumer stopped");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _cancellationTokenSource?.Cancel();
            _consumer?.Close();
            _consumer?.Dispose();
            _adminClient?.Dispose();
            _cancellationTokenSource?.Dispose();

            _disposed = true;
        }
    }
}