using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Dispositivos.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;

namespace Dispositivos.Infrastructure.Services;

public class PersonalUnlockDoorKafkaConsumerConfig
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string GroupId { get; set; } = "dispositivos-personal-unlock-group";
    public string Topic { get; set; } = "habitacion.personal-unlock";
    public string AutoOffsetReset { get; set; } = "Earliest";
    public bool EnableAutoCommit { get; set; } = true;
    public int AutoCommitIntervalMs { get; set; } = 5000;
    public int SessionTimeoutMs { get; set; } = 30000;
    public int MaxPollIntervalMs { get; set; } = 300000;
    public string PersonalAccesoProducerTopic { get; set; } = "habitacion.personal-acceso";
}

public class PersonalUnlockDoorKafkaConsumer : BackgroundService
{
    private readonly PersonalUnlockDoorKafkaConsumerConfig _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PersonalUnlockDoorKafkaConsumer> _logger;
    private IConsumer<string, string>? _consumer;
    private IAdminClient? _adminClient;
    private IProducer<string, string>? _producer;

    public PersonalUnlockDoorKafkaConsumer(
        PersonalUnlockDoorKafkaConsumerConfig config,
        IServiceScopeFactory scopeFactory,
        ILogger<PersonalUnlockDoorKafkaConsumer> logger)
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

        var adminConfig = new AdminClientConfig { BootstrapServers = _config.BootstrapServers };
        _adminClient = new AdminClientBuilder(adminConfig).Build();

        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = _config.BootstrapServers,
            Acks = Acks.Leader
        }).Build();

        EnsureTopicExists();

        _consumer.Subscribe(_config.Topic);
        _logger.LogInformation("PersonalUnlockDoorKafkaConsumer started. Listening on topic: {Topic}", _config.Topic);

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
            _logger.LogInformation("PersonalUnlockDoorKafkaConsumer stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in PersonalUnlockDoorKafkaConsumer");
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
            var unlockEvent = JsonSerializer.Deserialize<PersonalUnlockDoorEvent>(messageValue, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (unlockEvent == null)
            {
                _logger.LogWarning("Could not deserialize PersonalUnlockDoorEvent: {Message}", messageValue);
                return;
            }

            await HandlePersonalUnlockAsync(unlockEvent, cancellationToken);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing PersonalUnlockDoorEvent");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PersonalUnlockDoorEvent");
        }
    }

    private async Task HandlePersonalUnlockAsync(PersonalUnlockDoorEvent unlockEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing PersonalUnlockDoorEvent for personal {PersonalId}, habitacion {HabitacionId}",
            unlockEvent.PersonalId, unlockEvent.HabitacionId);

        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var tbDeviceService = scope.ServiceProvider.GetRequiredService<ITbDeviceService>();

        var cerraduras = await unitOfWork.CerradurasInteligente.GetByHabitacionId(unlockEvent.HabitacionId);
        var activeCerradura = cerraduras.FirstOrDefault(c => c.EstaActiva);

        if (activeCerradura == null)
        {
            _logger.LogWarning("No se encontró cerradura activa para habitacion {HabitacionId}", unlockEvent.HabitacionId);
            return;
        }

        // ThingsBoard unlock — non-blocking: log warning and continue if unavailable
        try
        {
            var tbDeviceName = activeCerradura.DispositivoId.ToString();
            var tbDevice = await tbDeviceService.GetDeviceByNameAsync(tbDeviceName, cancellationToken);

            if (tbDevice == null || string.IsNullOrEmpty(tbDevice.Id))
            {
                _logger.LogWarning(
                    "Dispositivo {DispositivoId} no encontrado en ThingsBoard para habitacion {HabitacionId}",
                    activeCerradura.DispositivoId, unlockEvent.HabitacionId);
            }
            else
            {
                await tbDeviceService.SetSharedAttributesAsync(
                    tbDevice.Id,
                    new Dictionary<string, object> { { "lockState", "unlocked" } },
                    cancellationToken);

                _logger.LogInformation(
                    "lockState=unlocked aplicado en ThingsBoard para dispositivo {DispositivoId} (habitacion {HabitacionId}, personal {PersonalId})",
                    activeCerradura.DispositivoId, unlockEvent.HabitacionId, unlockEvent.PersonalId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al comunicarse con ThingsBoard para habitacion {HabitacionId}", unlockEvent.HabitacionId);
        }

        await RegistrarAccesoAsync(unitOfWork, activeCerradura.CerraduraId, unlockEvent, cancellationToken);
        await PublicarPersonalAccesoEventAsync(unlockEvent, cancellationToken);
    }

    private async Task RegistrarAccesoAsync(
        IUnitOfWork unitOfWork,
        int cerraduraId,
        PersonalUnlockDoorEvent unlockEvent,
        CancellationToken cancellationToken)
    {
        try
        {
            var registro = new Dispositivos.Domain.Entities.RegistrosAcceso
            {
                CerraduraId = cerraduraId,
                UsuarioId = unlockEvent.UsuarioId,
                FechaHoraAcceso = DateTime.UtcNow,
                TipoAcceso = "Personal",
                ResultadoAcceso = "Concedido",
                MotivoAcceso = $"Acceso de personal - {unlockEvent.NombrePersonal} (ID: {unlockEvent.PersonalId})",
                DireccionIp = unlockEvent.DireccionIp,
                InfoDispositivo = unlockEvent.InfoDispositivo,
                FueExitoso = true
            };

            await unitOfWork.RegistrosAcceso.AddAsync(registro, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "RegistrosAcceso creado para cerradura {CerraduraId}, personal {PersonalId}",
                cerraduraId, unlockEvent.PersonalId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear RegistrosAcceso para cerradura {CerraduraId}", cerraduraId);
        }
    }

    private async Task PublicarPersonalAccesoEventAsync(PersonalUnlockDoorEvent unlockEvent, CancellationToken cancellationToken)
    {
        try
        {
            if (unlockEvent.Huespedes.Count == 0)
            {
                _logger.LogInformation(
                    "No hay huéspedes activos para habitacion {HabitacionId}, omitiendo notificación",
                    unlockEvent.HabitacionId);
                return;
            }

            var accesoEvent = new PersonalAccesoHabitacionEvent
            {
                HabitacionId = unlockEvent.HabitacionId,
                NumeroHabitacion = unlockEvent.NumeroHabitacion,
                PersonalId = unlockEvent.PersonalId,
                NombrePersonal = unlockEvent.NombrePersonal,
                Huespedes = unlockEvent.Huespedes,
                FechaAcceso = DateTime.UtcNow
            };

            var message = new Message<string, string>
            {
                Key = unlockEvent.HabitacionId.ToString(),
                Value = JsonSerializer.Serialize(accesoEvent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            };

            await _producer!.ProduceAsync(_config.PersonalAccesoProducerTopic, message, cancellationToken);

            _logger.LogInformation(
                "PersonalAccesoHabitacionEvent publicado para habitacion {HabitacionId}, {Count} huespedes",
                unlockEvent.HabitacionId, unlockEvent.Huespedes.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publicando PersonalAccesoHabitacionEvent para habitacion {HabitacionId}", unlockEvent.HabitacionId);
        }
    }

    public override void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(5));
        _producer?.Dispose();
        _consumer?.Dispose();
        _adminClient?.Dispose();
        base.Dispose();
    }
}
