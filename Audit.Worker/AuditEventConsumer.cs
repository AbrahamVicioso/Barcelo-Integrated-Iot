using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;

namespace Audit.Worker;

public class AuditEventConsumer : BackgroundService
{
    private readonly AuditConsumerConfig _config;
    private readonly IAuditRepository _repository;
    private readonly ILogger<AuditEventConsumer> _logger;

    public AuditEventConsumer(
        AuditConsumerConfig config,
        IAuditRepository repository,
        ILogger<AuditEventConsumer> logger)
    {
        _config = config;
        _repository = repository;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Esperar a que Kafka esté disponible antes de iniciar
        await WaitForKafkaAsync(stoppingToken);
        if (stoppingToken.IsCancellationRequested) return;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _config.BootstrapServers,
            GroupId = _config.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            AutoCommitIntervalMs = 5000,
            SessionTimeoutMs = 30000,
            MaxPollIntervalMs = 300000
        };

        using var adminClient = new AdminClientBuilder(
            new AdminClientConfig { BootstrapServers = _config.BootstrapServers }).Build();

        EnsureTopicExists(adminClient);

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka error: {Reason}", e.Reason))
            .Build();

        consumer.Subscribe(_config.Topic);
        _logger.LogInformation("AuditEventConsumer iniciado. Escuchando topic: {Topic}", _config.Topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    if (result?.Message?.Value is not null)
                        await ProcessAsync(result.Message.Value, stoppingToken);
                }
                catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                {
                    // El topic aún no existe en el broker — esperar y reintentar sin llenar el log
                    _logger.LogWarning("Topic '{Topic}' aún no disponible en Kafka, reintentando en 5s...", _config.Topic);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consumiendo mensaje de Kafka");
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("AuditEventConsumer detenido");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado en AuditEventConsumer");
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task WaitForKafkaAsync(CancellationToken stoppingToken)
    {
        var delay = TimeSpan.FromSeconds(5);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var adminClient = new AdminClientBuilder(
                    new AdminClientConfig { BootstrapServers = _config.BootstrapServers }).Build();

                adminClient.GetMetadata(TimeSpan.FromSeconds(5));
                _logger.LogInformation("Kafka disponible en {Servers}", _config.BootstrapServers);
                return;
            }
            catch
            {
                _logger.LogWarning("Kafka no disponible en {Servers}, reintentando en {Delay}s...",
                    _config.BootstrapServers, delay.TotalSeconds);
                await Task.Delay(delay, stoppingToken);
            }
        }
    }

    private void EnsureTopicExists(IAdminClient adminClient)
    {
        try
        {
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            if (!metadata.Topics.Any(t => t.Topic == _config.Topic))
            {
                adminClient.CreateTopicsAsync(new[]
                {
                    new TopicSpecification
                    {
                        Name = _config.Topic,
                        NumPartitions = 1,
                        ReplicationFactor = 1
                    }
                }).GetAwaiter().GetResult();

                _logger.LogInformation("Topic {Topic} creado", _config.Topic);
            }
        }
        catch (CreateTopicsException ex) when (ex.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
        {
            // Ya existe, ignorar
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo verificar/crear el topic. Se intentará suscribir de todas formas.");
        }
    }

    private async Task ProcessAsync(string messageValue, CancellationToken ct)
    {
        try
        {
            var auditEvent = JsonSerializer.Deserialize<AuditEvent>(messageValue, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (auditEvent is null)
            {
                _logger.LogWarning("No se pudo deserializar AuditEvent: {Message}", messageValue);
                return;
            }

            await _repository.SaveAsync(auditEvent, ct);

            _logger.LogInformation(
                "Audit guardado: [{Servicio}] {Accion}/{TipoEntidad} usuario={UsuarioId} resultado={Resultado}",
                auditEvent.Servicio, auditEvent.Accion, auditEvent.TipoEntidad,
                auditEvent.UsuarioId, auditEvent.Resultado);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializando mensaje de auditoría");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando evento de auditoría");
        }
    }
}

public class AuditConsumerConfig
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string GroupId { get; set; } = "audit-worker-group";
    public string Topic { get; set; } = "audit.events";
}
