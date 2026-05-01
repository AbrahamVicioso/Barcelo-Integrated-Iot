using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Dispositivos.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;

namespace Dispositivos.Infrastructure.Services;

public class ReservaActividadKafkaConsumerConfig
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string GroupId { get; set; } = "dispositivos-actividad-group";
    public string Topic { get; set; } = "actividades.reserva-confirmada";
    public string AutoOffsetReset { get; set; } = "Earliest";
    public bool EnableAutoCommit { get; set; } = true;
    public int AutoCommitIntervalMs { get; set; } = 5000;
    public int SessionTimeoutMs { get; set; } = 30000;
    public int MaxPollIntervalMs { get; set; } = 300000;
}

public class ReservaActividadKafkaConsumer : BackgroundService
{
    private readonly ReservaActividadKafkaConsumerConfig _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservaActividadKafkaConsumer> _logger;
    private IConsumer<string, string>? _consumer;
    private IAdminClient? _adminClient;

    public ReservaActividadKafkaConsumer(
        ReservaActividadKafkaConsumerConfig config,
        IServiceScopeFactory scopeFactory,
        ILogger<ReservaActividadKafkaConsumer> logger)
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

        EnsureTopicExists();

        _consumer.Subscribe(_config.Topic);
        _logger.LogInformation("ReservaActividadKafkaConsumer started. Listening on topic: {Topic}", _config.Topic);

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
            _logger.LogInformation("ReservaActividadKafkaConsumer stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ReservaActividadKafkaConsumer");
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
            var evt = JsonSerializer.Deserialize<ReservaActividadConfirmadaEvent>(messageValue, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (evt == null)
            {
                _logger.LogWarning("Could not deserialize ReservaActividadConfirmadaEvent: {Message}", messageValue);
                return;
            }

            await HandleAsync(evt, cancellationToken);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing ReservaActividadConfirmadaEvent message");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ReservaActividadConfirmadaEvent");
        }
    }

    private async Task HandleAsync(ReservaActividadConfirmadaEvent evt, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Procesando ReservaActividadConfirmadaEvent: ReservaActividadId={ReservaActividadId}, ActividadId={ActividadId}, HuespedId={HuespedId}",
            evt.ReservaActividadId, evt.ActividadId, evt.HuespedId);

        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // 1. Find the active lock for this activity
        var cerradura = await unitOfWork.CerradurasInteligente
            .GetByActividadIdAsync(evt.ActividadId, cancellationToken);

        if (cerradura == null)
        {
            _logger.LogInformation(
                "Actividad {ActividadId} no tiene cerradura activa asignada. No se generará credencial.",
                evt.ActividadId);
            return;
        }

        // 2. Generate PIN and create credential
        var pin = GenerarPin();
        var fechaExpiracion = evt.FechaReserva.Date
            .Add(evt.HoraReserva)
            .AddMinutes(evt.DuracionMinutos ?? 60);

        var credencial = new Dispositivos.Domain.Entities.CredencialesAcceso
        {
            HuespedId = evt.HuespedId,
            ReservaActividadId = evt.ReservaActividadId,
            CodigoPin = pin,
            HashPin = GenerarHash(pin),
            FechaActivacion = evt.FechaReserva.Date.Add(evt.HoraReserva),
            FechaExpiracion = fechaExpiracion,
            EstaActiva = true,
            TipoCredencial = "Actividad",
            CreadoPor = "Sistema",
            NumeroUsos = 0,
            FechaCreacion = DateTime.UtcNow
        };

        await unitOfWork.CredencialesAcceso.AddAsync(credencial, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Credencial de actividad generada para HuespedId={HuespedId}, ReservaActividadId={ReservaActividadId}, CerraduraId={CerraduraId}",
            evt.HuespedId, evt.ReservaActividadId, cerradura.CerraduraId);

        // 3. Sync credentials to ThingsBoard
        try
        {
            var syncService = scope.ServiceProvider.GetRequiredService<ITbCredencialesSyncService>();
            await syncService.SyncByCerraduraIdAsync(cerradura.CerraduraId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sincronizando ThingsBoard para cerradura {CerraduraId}.", cerradura.CerraduraId);
        }

        // 4. Publish CredencialCreadaEvent for notification (email/push)
        try
        {
            var kafkaProducer = scope.ServiceProvider.GetRequiredService<ICredencialesKafkaProducer>();
            var notifEvent = new CredencialCreadaEvent
            {
                CredencialId = credencial.CredencialId,
                HuespedId = evt.HuespedId,
                ReservaActividadId = evt.ReservaActividadId,
                Email = evt.Email,
                NombreCompleto = evt.NombreCompleto,
                CodigoPin = pin,
                FechaActivacion = credencial.FechaActivacion,
                FechaExpiracion = credencial.FechaExpiracion,
                TipoCredencial = "Actividad",
                NombreActividad = evt.NombreActividad
            };
            await kafkaProducer.PublishCredencialCreadaAsync(notifEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publicando CredencialCreadaEvent para ReservaActividadId={ReservaActividadId}.", evt.ReservaActividadId);
        }
    }

    private static string GenerarPin()
        => RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

    private static string GenerarHash(string texto)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(texto);
        return Convert.ToBase64String(sha256.ComputeHash(bytes));
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        _adminClient?.Dispose();
        base.Dispose();
    }
}
