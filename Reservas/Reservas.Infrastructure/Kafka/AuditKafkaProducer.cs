using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;
using Notification.Domain.Interfaces;

namespace Reservas.Infrastructure.Kafka;

public class AuditKafkaProducer : IAuditProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly ILogger<AuditKafkaProducer> _logger;
    private bool _disposed;

    public AuditKafkaProducer(AuditKafkaProducerConfig config, ILogger<AuditKafkaProducer> logger)
    {
        _topic = config.Topic;
        _logger = logger;

        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = config.BootstrapServers,
            ClientId = config.ClientId ?? "reservas-audit",
            Acks = Acks.Leader
        })
        .SetErrorHandler((_, e) => _logger.LogError("Kafka audit producer error: {Reason}", e.Reason))
        .Build();
    }

    public async Task PublishAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = auditEvent.Servicio,
                Value = JsonSerializer.Serialize(auditEvent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            };

            var result = await _producer.ProduceAsync(_topic, message, cancellationToken);

            _logger.LogDebug(
                "Audit event [{Accion}/{TipoEntidad}] publicado en particion {Partition} offset {Offset}",
                auditEvent.Accion, auditEvent.TipoEntidad, result.Partition.Value, result.Offset.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publicando audit event [{Accion}/{TipoEntidad}]",
                auditEvent.Accion, auditEvent.TipoEntidad);
            // No re-throw: auditing must never break the main flow
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _producer?.Flush(TimeSpan.FromSeconds(5));
        _producer?.Dispose();
        _disposed = true;
    }
}

public class AuditKafkaProducerConfig
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string Topic { get; set; } = "audit.events";
    public string? ClientId { get; set; }
}
