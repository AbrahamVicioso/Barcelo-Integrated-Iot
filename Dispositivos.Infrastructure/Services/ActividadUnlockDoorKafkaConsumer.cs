using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Dispositivos.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;

namespace Dispositivos.Infrastructure.Services;

public class ActividadUnlockDoorKafkaConsumerConfig
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string GroupId { get; set; } = "dispositivos-actividad-unlock-group";
    public string Topic { get; set; } = "actividades.unlock-door";
    public string AutoOffsetReset { get; set; } = "Earliest";
    public bool EnableAutoCommit { get; set; } = true;
    public int AutoCommitIntervalMs { get; set; } = 5000;
    public int SessionTimeoutMs { get; set; } = 30000;
    public int MaxPollIntervalMs { get; set; } = 300000;
}

public class ActividadUnlockDoorKafkaConsumer : BackgroundService
{
    private readonly ActividadUnlockDoorKafkaConsumerConfig _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ActividadUnlockDoorKafkaConsumer> _logger;
    private IConsumer<string, string>? _consumer;
    private IAdminClient? _adminClient;

    public ActividadUnlockDoorKafkaConsumer(
        ActividadUnlockDoorKafkaConsumerConfig config,
        IServiceScopeFactory scopeFactory,
        ILogger<ActividadUnlockDoorKafkaConsumer> logger)
    {
        _config = config;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

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

        _adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _config.BootstrapServers }).Build();
        EnsureTopicExists();

        _consumer.Subscribe(_config.Topic);
        _logger.LogInformation("ActividadUnlockDoorKafkaConsumer started. Listening on topic: {Topic}", _config.Topic);

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
            _logger.LogInformation("ActividadUnlockDoorKafkaConsumer stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ActividadUnlockDoorKafkaConsumer");
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
        catch (CreateTopicsException ex) when (ex.Results[0].Error.Code == ErrorCode.TopicAlreadyExists) { }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error ensuring topic exists. Will attempt to subscribe anyway.");
        }
    }

    private async Task ProcessMessageAsync(string messageValue, CancellationToken cancellationToken)
    {
        try
        {
            var unlockEvent = JsonSerializer.Deserialize<ActividadUnlockDoorEvent>(messageValue, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (unlockEvent == null)
            {
                _logger.LogWarning("Could not deserialize ActividadUnlockDoorEvent: {Message}", messageValue);
                return;
            }

            await HandleUnlockAsync(unlockEvent, cancellationToken);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing ActividadUnlockDoorEvent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ActividadUnlockDoorEvent");
        }
    }

    private async Task HandleUnlockAsync(ActividadUnlockDoorEvent unlockEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing ActividadUnlockDoorEvent for actividad {ActividadId} ({NombreActividad})",
            unlockEvent.ActividadId, unlockEvent.NombreActividad);

        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var tbDeviceService = scope.ServiceProvider.GetRequiredService<ITbDeviceService>();

        var cerradura = await unitOfWork.CerradurasInteligente.GetByActividadIdAsync(unlockEvent.ActividadId, cancellationToken);

        if (cerradura == null || !cerradura.EstaActiva)
        {
            _logger.LogWarning("No se encontró cerradura activa para actividad {ActividadId}", unlockEvent.ActividadId);
            return;
        }

        try
        {
            var tbDeviceName = cerradura.DispositivoId.ToString();
            var tbDevice = await tbDeviceService.GetDeviceByNameAsync(tbDeviceName, cancellationToken);

            if (tbDevice == null || string.IsNullOrEmpty(tbDevice.Id))
            {
                _logger.LogWarning(
                    "Dispositivo {DispositivoId} no encontrado en ThingsBoard para actividad {ActividadId}",
                    cerradura.DispositivoId, unlockEvent.ActividadId);
            }
            else
            {
                await tbDeviceService.SetSharedAttributesAsync(
                    tbDevice.Id,
                    new Dictionary<string, object> { { "lockState", "unlocked" } },
                    cancellationToken);

                _logger.LogInformation(
                    "lockState=unlocked aplicado en ThingsBoard para dispositivo {DispositivoId} (actividad {ActividadId})",
                    cerradura.DispositivoId, unlockEvent.ActividadId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al comunicarse con ThingsBoard para actividad {ActividadId}", unlockEvent.ActividadId);
        }

        await RegistrarAccesoAsync(unitOfWork, cerradura.CerraduraId, unlockEvent, cancellationToken);
    }

    private async Task RegistrarAccesoAsync(
        IUnitOfWork unitOfWork,
        int cerraduraId,
        ActividadUnlockDoorEvent unlockEvent,
        CancellationToken cancellationToken)
    {
        try
        {
            var registro = new Dispositivos.Domain.Entities.RegistrosAcceso
            {
                CerraduraId = cerraduraId,
                CredencialId = unlockEvent.CredencialId,
                UsuarioId = unlockEvent.UsuarioId,
                FechaHoraAcceso = DateTime.UtcNow,
                TipoAcceso = unlockEvent.CredencialId.HasValue ? "PIN" : "JWT",
                ResultadoAcceso = "Concedido",
                MotivoAcceso = $"Desbloqueo actividad '{unlockEvent.NombreActividad}' (reserva {unlockEvent.ReservaActividadId})",
                DireccionIp = unlockEvent.DireccionIp,
                InfoDispositivo = unlockEvent.InfoDispositivo,
                FueExitoso = true
            };

            await unitOfWork.RegistrosAcceso.AddAsync(registro, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "RegistrosAcceso creado para cerradura {CerraduraId}, actividad {ActividadId}",
                cerraduraId, unlockEvent.ActividadId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear RegistrosAcceso para cerradura {CerraduraId}", cerraduraId);
        }
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        _adminClient?.Dispose();
        base.Dispose();
    }
}
