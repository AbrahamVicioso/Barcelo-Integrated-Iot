using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;
using Reservas.Application.Interfaces;

namespace Reservas.Infrastructure.Kafka;

public class ReservaActividadKafkaProducer : IReservaActividadKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly string _actividadUnlockDoorTopic;
    private readonly string _personalActividadUnlockDoorTopic;
    private readonly ILogger<ReservaActividadKafkaProducer> _logger;
    private bool _disposed;

    public ReservaActividadKafkaProducer(ReservaActividadKafkaProducerConfig config, ILogger<ReservaActividadKafkaProducer> logger)
    {
        _topic = config.Topic;
        _actividadUnlockDoorTopic = config.ActividadUnlockDoorTopic;
        _personalActividadUnlockDoorTopic = config.PersonalActividadUnlockDoorTopic;
        _logger = logger;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config.BootstrapServers,
            ClientId = config.ClientId ?? "reservas-actividad-api",
            Acks = Acks.Leader,
            EnableDeliveryReports = true
        };

        _producer = new ProducerBuilder<string, string>(producerConfig)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka producer error: {Reason}", e.Reason))
            .Build();
    }

    public async Task PublishReservaActividadConfirmadaAsync(ReservaActividadConfirmadaEvent evt, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = evt.ReservaActividadId.ToString(),
                Value = JsonSerializer.Serialize(evt, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            };

            var result = await _producer.ProduceAsync(_topic, message, cancellationToken);

            _logger.LogInformation(
                "Published ReservaActividadConfirmadaEvent for actividad {ActividadId}, huesped {HuespedId} to partition {Partition} at offset {Offset}",
                evt.ActividadId, evt.HuespedId, result.Partition.Value, result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to publish ReservaActividadConfirmadaEvent for reservaActividad {ReservaActividadId}", evt.ReservaActividadId);
            throw;
        }
    }

    public async Task PublishActividadUnlockDoorAsync(ActividadUnlockDoorEvent unlockEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = unlockEvent.ReservaActividadId.ToString(),
                Value = JsonSerializer.Serialize(unlockEvent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            };

            var result = await _producer.ProduceAsync(_actividadUnlockDoorTopic, message, cancellationToken);

            _logger.LogInformation(
                "Published ActividadUnlockDoorEvent for actividad {ActividadId}, reservaActividad {ReservaActividadId} to partition {Partition} at offset {Offset}",
                unlockEvent.ActividadId, unlockEvent.ReservaActividadId, result.Partition.Value, result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to publish ActividadUnlockDoorEvent for reservaActividad {ReservaActividadId}", unlockEvent.ReservaActividadId);
            throw;
        }
    }

    public async Task PublishPersonalActividadUnlockDoorAsync(PersonalActividadUnlockDoorEvent unlockEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = unlockEvent.ActividadId.ToString(),
                Value = JsonSerializer.Serialize(unlockEvent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            };

            var result = await _producer.ProduceAsync(_personalActividadUnlockDoorTopic, message, cancellationToken);

            _logger.LogInformation(
                "Published PersonalActividadUnlockDoorEvent for personal {PersonalId}, actividad {ActividadId} to partition {Partition} at offset {Offset}",
                unlockEvent.PersonalId, unlockEvent.ActividadId, result.Partition.Value, result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to publish PersonalActividadUnlockDoorEvent for personal {PersonalId}, actividad {ActividadId}",
                unlockEvent.PersonalId, unlockEvent.ActividadId);
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

public class ReservaActividadKafkaProducerConfig
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string Topic { get; set; } = "actividades.reserva-confirmada";
    public string ActividadUnlockDoorTopic { get; set; } = "actividades.unlock-door";
    public string PersonalActividadUnlockDoorTopic { get; set; } = "actividades.personal-unlock";
    public string? ClientId { get; set; }
}
