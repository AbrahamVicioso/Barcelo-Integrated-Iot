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
    public class PersonalAccesoHabitacionEventConsumer : NotificacionHandlerBase, IKafkaConsumer
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IAdminClient _adminClient;
        private readonly PersonalAccesoHabitacionConsumerConfig _config;
        private readonly ILogger<PersonalAccesoHabitacionEventConsumer> _logger;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _consumeTask;
        private bool _disposed;

        public bool IsRunning { get; private set; }

        public PersonalAccesoHabitacionEventConsumer(
            PersonalAccesoHabitacionConsumerConfig config,
            IPreferenciasRepository preferenciasRepo,
            INotificacionesRepository notificacionesRepo,
            AuthApiClient authApiClient,
            IEmailService emailService,
            IPushNotificationService pushService,
            ILogger<PersonalAccesoHabitacionEventConsumer> logger)
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

            _logger.LogInformation("PersonalAccesoHabitacionEventConsumer started. Listening to topic: {Topic}", _config.Topic);
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
                _logger.LogInformation("PersonalAccesoHabitacionEventConsumer stopping due to cancellation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in PersonalAccesoHabitacionEventConsumer");
            }
        }

        private async Task ProcessMessageAsync(string messageValue, CancellationToken cancellationToken)
        {
            try
            {
                var accesoEvent = JsonSerializer.Deserialize<PersonalAccesoHabitacionEvent>(messageValue,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (accesoEvent == null)
                {
                    _logger.LogWarning("Could not deserialize PersonalAccesoHabitacionEvent");
                    return;
                }

                await ProcessAccesoEventAsync(accesoEvent, cancellationToken);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing PersonalAccesoHabitacionEvent: {Message}", messageValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PersonalAccesoHabitacionEvent");
            }
        }

        private async Task ProcessAccesoEventAsync(PersonalAccesoHabitacionEvent accesoEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Procesando PersonalAccesoHabitacionEvent para habitacion {HabitacionId}, personal {NombrePersonal}, {Count} huespedes",
                accesoEvent.HabitacionId, accesoEvent.NombrePersonal, accesoEvent.Huespedes.Count);

            foreach (var huesped in accesoEvent.Huespedes)
            {
                if (string.IsNullOrEmpty(huesped.Email))
                {
                    _logger.LogWarning("HuespedId {HuespedId} no tiene email, omitiendo notificación", huesped.HuespedId);
                    continue;
                }

                try
                {
                    var emailBody = GenerarEmailAccesoPersonal(huesped, accesoEvent);
                    var emailNotification = new EmailNotification
                    {
                        To = huesped.Email,
                        Subject = $"Alerta: Personal ingresó a tu habitación {accesoEvent.NumeroHabitacion}",
                        Body = emailBody,
                        IsHtml = true
                    };

                    var pushNotification = new PushNotification
                    {
                        Topic = NtfyTopicHelper.GetUserTopic(huesped.Email),
                        Title = $"Acceso a tu habitación {accesoEvent.NumeroHabitacion}",
                        Message = $"El personal {accesoEvent.NombrePersonal} ingresó a tu habitación el {accesoEvent.FechaAcceso:dd/MM/yyyy HH:mm}.",
                        Priority = PushPriority.High,
                        Tags = ["hotel", "warning"]
                    };

                    // Usar métodos de la clase base que verifican preferencias
                    await EnviarNotificacionCompletaAsync(
                        huesped.Email,
                        "AccesoPersonal",
                        emailNotification,
                        pushNotification,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error enviando notificación de acceso personal a {Email}, habitacion {NumeroHabitacion}",
                        huesped.Email, accesoEvent.NumeroHabitacion);
                }
            }
        }

        private string GenerarEmailAccesoPersonal(HuespedCheckInInfo huesped, PersonalAccesoHabitacionEvent ev)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #e53935; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .info-box {{ background-color: #fff; border: 1px solid #ddd; padding: 15px; margin: 15px 0; border-radius: 4px; }}
        .alert {{ background-color: #fff3cd; border: 1px solid #ffc107; padding: 15px; border-radius: 4px; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Notificación de acceso a tu habitación</h1>
        </div>
        <div class='content'>
            <p>Estimado/a <strong>{huesped.NombreCompleto}</strong>,</p>
            <p>Te informamos que un miembro del personal del hotel ha ingresado a tu habitación:</p>
            <div class='info-box'>
                <p><strong>Habitación:</strong> {ev.NumeroHabitacion}</p>
                <p><strong>Personal:</strong> {ev.NombrePersonal}</p>
                <p><strong>Fecha y hora:</strong> {ev.FechaAcceso:dd/MM/yyyy HH:mm} UTC</p>
            </div>
            <div class='alert'>
                Si no solicitaste este servicio o tienes alguna duda, por favor contacta inmediatamente con la recepción del hotel.
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
            if (!IsRunning) return Task.CompletedTask;

            _cancellationTokenSource?.Cancel();
            _consumeTask?.Wait(cancellationToken);
            _consumer.Close();
            IsRunning = false;

            _logger.LogInformation("PersonalAccesoHabitacionEventConsumer stopped");
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
