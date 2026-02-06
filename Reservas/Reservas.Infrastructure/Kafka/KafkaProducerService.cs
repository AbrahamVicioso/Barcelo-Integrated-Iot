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
        private readonly ILogger<ReservaKafkaProducer> _logger;
        private bool _disposed;

        public ReservaKafkaProducer(KafkaProducerConfig config, ILogger<ReservaKafkaProducer> logger)
        {
            _topic = config.Topic;
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
        public string Topic { get; set; } = "notifications";
        public string? ClientId { get; set; }
    }
}
