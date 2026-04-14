using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;
using Usuarios.Application.Interfaces;

namespace Usuarios.API.Services;

public class PermisoHabitacionKafkaProducer : IPermisoHabitacionSyncProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly ILogger<PermisoHabitacionKafkaProducer> _logger;
    private bool _disposed;

    public PermisoHabitacionKafkaProducer(
        PermisoHabitacionKafkaProducerConfig config,
        ILogger<PermisoHabitacionKafkaProducer> logger)
    {
        _topic = config.Topic;
        _logger = logger;

        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = config.BootstrapServers,
            Acks = Acks.Leader
        })
        .SetErrorHandler((_, e) => _logger.LogError("Kafka permiso producer error: {Reason}", e.Reason))
        .Build();
    }

    public async Task PublishAsync(int habitacionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var evt = new PermisoPersonalCreadoEvent { HabitacionId = habitacionId };
            var message = new Message<string, string>
            {
                Key = habitacionId.ToString(),
                Value = JsonSerializer.Serialize(evt, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            };

            await _producer.ProduceAsync(_topic, message, cancellationToken);
            _logger.LogDebug("PermisoPersonalCreadoEvent publicado para Habitacion {HabitacionId}.", habitacionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publicando PermisoPersonalCreadoEvent para Habitacion {HabitacionId}.", habitacionId);
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

public class PermisoHabitacionKafkaProducerConfig
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string Topic { get; set; } = "habitacion.permiso-personal";
}
