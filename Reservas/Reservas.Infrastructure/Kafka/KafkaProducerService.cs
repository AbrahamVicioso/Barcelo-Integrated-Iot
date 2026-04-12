using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;
using Reservas.Application.Interfaces;

namespace Reservas.Infrastructure.Kafka
{
    public class ReservaKafkaProducer : IReservaKafkaProducer, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;
        private readonly string _unlockDoorTopic;
        private readonly string _checkInRealizadoTopic;
        private readonly string _personalUnlockDoorTopic;
        private readonly ILogger<ReservaKafkaProducer> _logger;
        private bool _disposed;

        public ReservaKafkaProducer(KafkaProducerConfig config, ILogger<ReservaKafkaProducer> logger)
        {
            _topic = config.Topic;
            _unlockDoorTopic = config.UnlockDoorTopic;
            _checkInRealizadoTopic = config.CheckInRealizadoTopic;
            _personalUnlockDoorTopic = config.PersonalUnlockDoorTopic;
            _logger = logger;

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config.BootstrapServers,
                ClientId = config.ClientId ?? "reservas-api",
                Acks = Acks.Leader,
                EnableDeliveryReports = true
            };

            _producer = new ProducerBuilder<string, string>(producerConfig)
                .SetErrorHandler((_, e) => _logger.LogError("Kafka producer error: {Reason}", e.Reason))
                .Build();
        }

        public async Task PublishReservaCreadaAsync(ReservaCreadaEvent reservaEvent, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new Message<string, string>
                {
                    Key = reservaEvent.NumeroReserva,
                    Value = JsonSerializer.Serialize(reservaEvent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })
                };

                var result = await _producer.ProduceAsync(_topic, message, cancellationToken);

                _logger.LogInformation("Published ReservaCreadaEvent for {NumeroReserva} to partition {Partition} at offset {Offset}",
                    reservaEvent.NumeroReserva, result.Partition.Value, result.Offset.Value);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to publish ReservaCreadaEvent for {NumeroReserva}", reservaEvent.NumeroReserva);
                throw;
            }
        }

        public async Task PublishUnlockDoorAsync(UnlockDoorEvent unlockDoorEvent, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new Message<string, string>
                {
                    Key = unlockDoorEvent.NumeroReserva,
                    Value = JsonSerializer.Serialize(unlockDoorEvent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })
                };

                var result = await _producer.ProduceAsync(_unlockDoorTopic, message, cancellationToken);

                _logger.LogInformation("Published UnlockDoorEvent for reserva {NumeroReserva} habitacion {HabitacionId} to partition {Partition} at offset {Offset}",
                    unlockDoorEvent.NumeroReserva, unlockDoorEvent.HabitacionId, result.Partition.Value, result.Offset.Value);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to publish UnlockDoorEvent for reserva {NumeroReserva}", unlockDoorEvent.NumeroReserva);
                throw;
            }
        }

        public async Task PublishCheckInRealizadoAsync(CheckInRealizadoEvent checkInEvent, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new Message<string, string>
                {
                    Key = checkInEvent.NumeroReserva,
                    Value = JsonSerializer.Serialize(checkInEvent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })
                };

                var result = await _producer.ProduceAsync(_checkInRealizadoTopic, message, cancellationToken);

                _logger.LogInformation(
                    "Published CheckInRealizadoEvent for reserva {NumeroReserva} ({Count} huespedes) to partition {Partition} at offset {Offset}",
                    checkInEvent.NumeroReserva, checkInEvent.Huespedes.Count, result.Partition.Value, result.Offset.Value);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to publish CheckInRealizadoEvent for reserva {NumeroReserva}", checkInEvent.NumeroReserva);
                throw;
            }
        }

        public async Task PublishPersonalUnlockDoorAsync(PersonalUnlockDoorEvent personalUnlockEvent, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new Message<string, string>
                {
                    Key = personalUnlockEvent.HabitacionId.ToString(),
                    Value = JsonSerializer.Serialize(personalUnlockEvent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })
                };

                var result = await _producer.ProduceAsync(_personalUnlockDoorTopic, message, cancellationToken);

                _logger.LogInformation(
                    "Published PersonalUnlockDoorEvent for personal {PersonalId}, habitacion {HabitacionId} to partition {Partition} at offset {Offset}",
                    personalUnlockEvent.PersonalId, personalUnlockEvent.HabitacionId, result.Partition.Value, result.Offset.Value);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to publish PersonalUnlockDoorEvent for personal {PersonalId}, habitacion {HabitacionId}",
                    personalUnlockEvent.PersonalId, personalUnlockEvent.HabitacionId);
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
            _disposed = true;
        }
    }

    public class KafkaProducerConfig
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string Topic { get; set; } = "reservas";
        public string UnlockDoorTopic { get; set; } = "dispositivos.unlock-door";
        public string CheckInRealizadoTopic { get; set; } = "reservas.checkin-realizado";
        public string PersonalUnlockDoorTopic { get; set; } = "habitacion.personal-unlock";
        public string? ClientId { get; set; }
    }
}
