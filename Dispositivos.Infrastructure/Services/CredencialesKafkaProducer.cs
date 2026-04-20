using System.Text.Json;
using Confluent.Kafka;
using Dispositivos.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;

namespace Dispositivos.Infrastructure.Services;

public class CredencialesKafkaProducer : ICredencialesKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly ILogger<CredencialesKafkaProducer> _logger;
    private bool _disposed;

    public CredencialesKafkaProducer(CredencialesKafkaProducerConfig config, ILogger<CredencialesKafkaProducer> logger)
    {
        _topic = config.Topic;
        _logger = logger;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config.BootstrapServers,
            ClientId = config.ClientId ?? "dispositivos-api",
            Acks = Acks.Leader,
            EnableDeliveryReports = true
        };

        _producer = new ProducerBuilder<string, string>(producerConfig)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka producer error: {Reason}", e.Reason))
            .Build();
    }

    public async Task PublishCredencialCreadaAsync(CredencialCreadaEvent credencialEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = credencialEvent.CredencialId.ToString();
            var message = new Message<string, string>
            {
                Key = key,
                Value = JsonSerializer.Serialize(credencialEvent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            };

            var result = await _producer.ProduceAsync(_topic, message, cancellationToken);

            _logger.LogInformation(
                "Published CredencialCreadaEvent for credencial {CredencialId} to partition {Partition} at offset {Offset}",
                credencialEvent.CredencialId, result.Partition.Value, result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to publish CredencialCreadaEvent for credencial {CredencialId}", credencialEvent.CredencialId);
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
        _disposed = true;
    }
}