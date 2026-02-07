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
    public class ReservaCreadaEventConsumer : IKafkaConsumer
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IAdminClient _adminClient;
        private readonly IEmailService _emailService;
        private readonly ReservaCreadaConsumerConfig _config;
        private readonly ILogger<ReservaCreadaEventConsumer> _logger;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _consumeTask;
        private bool _disposed;

        public bool IsRunning { get; private set; }

        public ReservaCreadaEventConsumer(
            ReservaCreadaConsumerConfig config,
            IEmailService emailService,
            ILogger<ReservaCreadaEventConsumer> logger)
        {
            _config = config;
            _emailService = emailService;
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

            _logger.LogInformation("ReservaCreadaEventConsumer started. Listening to topic: {Topic}", _config.Topic);

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
                _logger.LogInformation("ReservaCreadaEventConsumer stopping due to cancellation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ReservaCreadaEventConsumer");
            }
        }

        private async Task ProcessMessageAsync(string messageValue, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Received message: {Message}", messageValue);

                var reservaCreadaEvent = JsonSerializer.Deserialize<ReservaCreadaEvent>(messageValue, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (reservaCreadaEvent != null)
                {
                    await ProcessReservaCreadaEventAsync(reservaCreadaEvent, cancellationToken);
                    return;
                }

                _logger.LogWarning("Could not deserialize message as ReservaCreadaEvent");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing message: {Message}", messageValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing reserva created event");
            }
        }

        private async Task ProcessReservaCreadaEventAsync(ReservaCreadaEvent reservaEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing ReservaCreadaEvent for reservation: {NumeroReserva}", reservaEvent.NumeroReserva);

            var emailBody = GenerateReservaCreadaEmailBody(reservaEvent);

            var emailNotification = new EmailNotification
            {
                To = reservaEvent.Email,
                Subject = $"Confirmación de Reserva - {reservaEvent.NumeroReserva}",
                Body = emailBody,
                IsHtml = true
            };

            var sent = await _emailService.SendEmailAsync(emailNotification, cancellationToken);

            if (sent)
            {
                _logger.LogInformation("Reserva created email sent successfully to {Email} for reservation {NumeroReserva}", 
                    reservaEvent.Email, reservaEvent.NumeroReserva);
            }
            else
            {
                _logger.LogError("Failed to send reserva created email to {Email} for reservation {NumeroReserva}", 
                    reservaEvent.Email, reservaEvent.NumeroReserva);
            }
        }

        private string GenerateReservaCreadaEmailBody(ReservaCreadaEvent reservaEvent)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .reservation-details {{ background-color: #fff; border: 1px solid #ddd; padding: 15px; margin: 15px 0; }}
        .reservation-number {{ font-family: monospace; font-size: 20px; color: #28a745; font-weight: bold; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Reserva Confirmada</h1>
        </div>
        <div class='content'>
            <p>¡Tu reserva ha sido confirmada exitosamente!</p>
            <div class='reservation-details'>
                <p><strong>Número de Reserva:</strong> <span class='reservation-number'>{reservaEvent.NumeroReserva}</span></p>
                <p><strong>Hotel:</strong> {reservaEvent.HotelNombre}</p>
                <p><strong>Habitación:</strong> {reservaEvent.HabitacionNumero}</p>
                <p><strong>Check-in:</strong> {reservaEvent.FechaCheckIn:dd/MM/yyyy}</p>
                <p><strong>Check-out:</strong> {reservaEvent.FechaCheckOut:dd/MM/yyyy}</p>
                <p><strong>Monto Total:</strong> ${reservaEvent.MontoTotal:N2}</p>
            </div>
            <p>Gracias por elegir Barcelo Integrated IoT para tu estadía.</p>
            <p>Si tienes alguna pregunta o necesitas asistencia, no dudes en contactar a nuestro equipo de soporte.</p>
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

            _logger.LogInformation("ReservaCreadaEventConsumer stopped");

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
