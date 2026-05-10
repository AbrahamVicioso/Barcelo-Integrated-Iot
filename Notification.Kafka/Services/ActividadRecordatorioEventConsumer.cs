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
    public class ActividadRecordatorioEventConsumer : NotificacionHandlerBase, IKafkaConsumer
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IAdminClient _adminClient;
        private readonly ActividadRecordatorioConsumerConfig _config;
        private readonly ILogger<ActividadRecordatorioEventConsumer> _logger;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _consumeTask;
        private bool _disposed;

        public bool IsRunning { get; private set; }

        public ActividadRecordatorioEventConsumer(
            ActividadRecordatorioConsumerConfig config,
            IPreferenciasRepository preferenciasRepo,
            INotificacionesRepository notificacionesRepo,
            AuthApiClient authApiClient,
            IEmailService emailService,
            IPushNotificationService pushService,
            ILogger<ActividadRecordatorioEventConsumer> logger)
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

            _logger.LogInformation("ActividadRecordatorioEventConsumer started. Listening to topic: {Topic}", _config.Topic);
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
                _logger.LogInformation("ActividadRecordatorioEventConsumer stopping due to cancellation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ActividadRecordatorioEventConsumer");
            }
        }

        private async Task ProcessMessageAsync(string messageValue, CancellationToken cancellationToken)
        {
            try
            {
                var evt = JsonSerializer.Deserialize<ActividadRecordatorioEvent>(messageValue,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (evt == null)
                {
                    _logger.LogWarning("Could not deserialize ActividadRecordatorioEvent");
                    return;
                }

                await ProcessRecordatorioAsync(evt, cancellationToken);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing ActividadRecordatorioEvent: {Message}", messageValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ActividadRecordatorioEvent");
            }
        }

        private async Task ProcessRecordatorioAsync(ActividadRecordatorioEvent evt, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Procesando ActividadRecordatorioEvent para reservaActividad {ReservaActividadId}, huesped {HuespedId}",
                evt.ReservaActividadId, evt.HuespedId);

            if (string.IsNullOrEmpty(evt.Email))
            {
                _logger.LogWarning("ActividadRecordatorioEvent sin email para reservaActividad {ReservaActividadId}", evt.ReservaActividadId);
                return;
            }

            var horaInicio = evt.FechaReserva.Date.Add(evt.HoraReserva);

            var emailNotification = new EmailNotification
            {
                To = evt.Email,
                Subject = $"Recordatorio: {evt.NombreActividad} — ¡Te esperamos!",
                Body = GenerarEmailRecordatorio(evt, horaInicio),
                IsHtml = true
            };

            var pushNotification = new PushNotification
            {
                Topic = NtfyTopicHelper.GetUserTopic(evt.Email),
                Title = $"Tu actividad empieza pronto: {evt.NombreActividad}",
                Message = $"{evt.NombreActividad} comienza a las {horaInicio:HH:mm} en {evt.Ubicacion}. ¡Te esperamos!",
                Priority = PushPriority.High,
                Tags = ["bell", "hotel"]
            };

            await EnviarNotificacionCompletaAsync(
                evt.Email,
                "RecordatorioActividad",
                emailNotification,
                pushNotification,
                cancellationToken);
        }

        private string GenerarEmailRecordatorio(ActividadRecordatorioEvent evt, DateTime horaInicio)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #ff9800; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .info-box {{ background-color: #fff; border: 1px solid #ddd; padding: 15px; margin: 15px 0; border-radius: 4px; }}
        .highlight {{ font-size: 24px; font-weight: bold; color: #ff9800; text-align: center; margin: 10px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Recordatorio de actividad</h1>
        </div>
        <div class='content'>
            <p>Hola <strong>{evt.NombreCompleto}</strong>,</p>
            <p>Tu actividad está por comenzar:</p>
            <div class='highlight'>{evt.NombreActividad}</div>
            <div class='info-box'>
                <p><strong>Fecha:</strong> {evt.FechaReserva:dd/MM/yyyy}</p>
                <p><strong>Hora de inicio:</strong> {horaInicio:HH:mm}</p>
                <p><strong>Ubicación:</strong> {evt.Ubicacion}</p>
                <p><strong>Personas:</strong> {evt.NumeroPersonas}</p>
            </div>
            {(string.IsNullOrEmpty(evt.Descripcion) ? "" : $"<p>{evt.Descripcion}</p>")}
            <p>¡Te esperamos en <strong>{evt.Ubicacion}</strong>!</p>
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

            _logger.LogInformation("ActividadRecordatorioEventConsumer stopped");
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
