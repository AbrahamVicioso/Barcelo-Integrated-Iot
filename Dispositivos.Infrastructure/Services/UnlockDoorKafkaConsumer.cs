using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Dispositivos.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Domain.Events;

namespace Dispositivos.Infrastructure.Services;

public class UnlockDoorKafkaConsumerConfig
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string GroupId { get; set; } = "dispositivos-unlock-door-group";
    public string Topic { get; set; } = "dispositivos.unlock-door";
    public string AutoOffsetReset { get; set; } = "Earliest";
    public bool EnableAutoCommit { get; set; } = true;
    public int AutoCommitIntervalMs { get; set; } = 5000;
    public int SessionTimeoutMs { get; set; } = 30000;
    public int MaxPollIntervalMs { get; set; } = 300000;
}

public class UnlockDoorKafkaConsumer : BackgroundService
{
    private readonly UnlockDoorKafkaConsumerConfig _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UnlockDoorKafkaConsumer> _logger;
    private IConsumer<string, string>? _consumer;
    private IAdminClient? _adminClient;

    public UnlockDoorKafkaConsumer(
        UnlockDoorKafkaConsumerConfig config,
        IServiceScopeFactory scopeFactory,
        ILogger<UnlockDoorKafkaConsumer> logger)
    {
        _config = config;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await WaitForKafkaAsync(stoppingToken);
        if (stoppingToken.IsCancellationRequested) return;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _config.BootstrapServers,
            GroupId = _config.GroupId,
            AutoOffsetReset = Enum.Parse<AutoOffsetReset>(_config.AutoOffsetReset, true),
            EnableAutoCommit = _config.EnableAutoCommit,
            AutoCommitIntervalMs = _config.AutoCommitIntervalMs,
            SessionTimeoutMs = _config.SessionTimeoutMs,
            MaxPollIntervalMs = _config.MaxPollIntervalMs
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka error: {Reason}", e.Reason))
            .Build();

        var adminConfig = new AdminClientConfig { BootstrapServers = _config.BootstrapServers };
        _adminClient = new AdminClientBuilder(adminConfig).Build();

        EnsureTopicExists();

        _consumer.Subscribe(_config.Topic);
        _logger.LogInformation("UnlockDoorKafkaConsumer started. Listening on topic: {Topic}", _config.Topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);
                    if (result != null)
                        await ProcessMessageAsync(result.Message.Value, stoppingToken);
                }
                catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                {
                    _logger.LogWarning("Topic '{Topic}' aún no disponible, reintentando en 5s...", _config.Topic);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from Kafka");
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("UnlockDoorKafkaConsumer stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in UnlockDoorKafkaConsumer");
        }
        finally
        {
            _consumer.Close();
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

    private void EnsureTopicExists()
    {
        try
        {
            var metadata = _adminClient!.GetMetadata(TimeSpan.FromSeconds(10));
            if (!metadata.Topics.Any(t => t.Topic == _config.Topic))
            {
                _adminClient.CreateTopicsAsync(new[]
                {
                    new TopicSpecification { Name = _config.Topic, NumPartitions = 1, ReplicationFactor = 1 }
                }).GetAwaiter().GetResult();

                _logger.LogInformation("Topic {Topic} created", _config.Topic);
            }
        }
        catch (CreateTopicsException ex) when (ex.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
        {
            // Already exists, ignore
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error ensuring topic exists. Will attempt to subscribe anyway.");
        }
    }

    private async Task ProcessMessageAsync(string messageValue, CancellationToken cancellationToken)
    {
        try
        {
            var unlockEvent = JsonSerializer.Deserialize<UnlockDoorEvent>(messageValue, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (unlockEvent == null)
            {
                _logger.LogWarning("Could not deserialize UnlockDoorEvent from message: {Message}", messageValue);
                return;
            }

            await HandleUnlockDoorAsync(unlockEvent, cancellationToken);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing unlock door message");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing unlock door event");
        }
    }

    private async Task HandleUnlockDoorAsync(UnlockDoorEvent unlockEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing UnlockDoorEvent for reserva {NumeroReserva}, habitacion {HabitacionId}",
            unlockEvent.NumeroReserva, unlockEvent.HabitacionId);

        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var tbDeviceService = scope.ServiceProvider.GetRequiredService<ITbDeviceService>();

        var cerraduras = await unitOfWork.CerradurasInteligente.GetByHabitacionId(unlockEvent.HabitacionId);

        var activeCerradura = cerraduras.FirstOrDefault(c => c.EstaActiva);
        if (activeCerradura == null)
        {
            _logger.LogWarning(
                "No se encontro cerradura activa para habitacion {HabitacionId}",
                unlockEvent.HabitacionId);
            return;
        }

        // ThingsBoard device name is the DispositivoId (set during device creation)
        var tbDeviceName = activeCerradura.DispositivoId.ToString();
        var tbDevice = await tbDeviceService.GetDeviceByNameAsync(tbDeviceName, cancellationToken);

        if (tbDevice == null || string.IsNullOrEmpty(tbDevice.Id))
        {
            _logger.LogWarning(
                "Dispositivo {DispositivoId} no encontrado en ThingsBoard para habitacion {HabitacionId}",
                activeCerradura.DispositivoId, unlockEvent.HabitacionId);
            return;
        }

        await tbDeviceService.SetSharedAttributesAsync(
            tbDevice.Id,
            new Dictionary<string, object> { { "lockState", "unlocked" } },
            cancellationToken);

        _logger.LogInformation(
            "lockState=unlocked aplicado en ThingsBoard para dispositivo {DispositivoId} (habitacion {HabitacionId}, reserva {NumeroReserva})",
            activeCerradura.DispositivoId, unlockEvent.HabitacionId, unlockEvent.NumeroReserva);
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        _adminClient?.Dispose();
        base.Dispose();
    }
}
