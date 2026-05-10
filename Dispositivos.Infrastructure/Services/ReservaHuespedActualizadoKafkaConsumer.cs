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

public class ReservaHuespedActualizadoKafkaConsumerConfig
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string GroupId { get; set; } = "dispositivos-huesped-actualizado-group";
    public string Topic { get; set; } = "reservas.huesped-actualizado";
    public string AutoOffsetReset { get; set; } = "Earliest";
    public bool EnableAutoCommit { get; set; } = true;
    public int AutoCommitIntervalMs { get; set; } = 5000;
    public int SessionTimeoutMs { get; set; } = 30000;
    public int MaxPollIntervalMs { get; set; } = 300000;
}

public class ReservaHuespedActualizadoKafkaConsumer : BackgroundService
{
    private readonly ReservaHuespedActualizadoKafkaConsumerConfig _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservaHuespedActualizadoKafkaConsumer> _logger;
    private IConsumer<string, string>? _consumer;
    private IAdminClient? _adminClient;

    public ReservaHuespedActualizadoKafkaConsumer(
        ReservaHuespedActualizadoKafkaConsumerConfig config,
        IServiceScopeFactory scopeFactory,
        ILogger<ReservaHuespedActualizadoKafkaConsumer> logger)
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
        _logger.LogInformation("ReservaHuespedActualizadoKafkaConsumer started. Listening on topic: {Topic}", _config.Topic);

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
            _logger.LogInformation("ReservaHuespedActualizadoKafkaConsumer stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ReservaHuespedActualizadoKafkaConsumer");
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
            var evt = JsonSerializer.Deserialize<ReservaHuespedActualizadoEvent>(messageValue, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (evt == null)
            {
                _logger.LogWarning("Could not deserialize ReservaHuespedActualizadoEvent: {Message}", messageValue);
                return;
            }

            await HandleAsync(evt, cancellationToken);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing ReservaHuespedActualizadoEvent message");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ReservaHuespedActualizadoEvent");
        }
    }

    private async Task HandleAsync(ReservaHuespedActualizadoEvent evt, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Procesando ReservaHuespedActualizadoEvent para reserva {ReservaId}, {AuthCount} autorizados de {TotalCount} total",
            evt.ReservaId, evt.HuespedesAutorizados.Count, evt.TodosHuespedes.Count);

        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Credenciales existentes para esta reserva (solo huéspedes)
        var credencialesExistentes = (await unitOfWork.CredencialesAcceso.GetByReservaIdAsync(evt.ReservaId)).ToList();

        // 1. Desactivar credenciales de huéspedes sin permiso o removidos de la reserva
        foreach (var credencial in credencialesExistentes.Where(c => c.EstaActiva))
        {
            if (credencial.HuespedId == null) continue;

            bool tienePermiso = evt.HuespedesAutorizados.Contains(credencial.HuespedId.Value);
            if (!tienePermiso)
            {
                credencial.EstaActiva = false;
                await unitOfWork.CredencialesAcceso.UpdateAsync(credencial, cancellationToken);
                _logger.LogInformation(
                    "Credencial {CredencialId} desactivada para HuespedId {HuespedId} (reserva {ReservaId})",
                    credencial.CredencialId, credencial.HuespedId, evt.ReservaId);
            }
        }

        // 2. Crear/reactivar credenciales para huéspedes autorizados sin credencial activa
        foreach (var huespedId in evt.HuespedesAutorizados)
        {
            var credencialExistente = credencialesExistentes.FirstOrDefault(c => c.HuespedId == huespedId);

            if (credencialExistente == null)
            {
                // Nunca tuvo credencial → crear nueva
                var pin = GenerarPin();
                var nuevaCredencial = new Dispositivos.Domain.Entities.CredencialesAcceso
                {
                    HuespedId = huespedId,
                    ReservaId = evt.ReservaId,
                    CodigoPin = pin,
                    HashPin = GenerarHash(pin),
                    FechaActivacion = evt.FechaCheckIn,
                    FechaExpiracion = evt.FechaCheckOut,
                    EstaActiva = true,
                    TipoCredencial = "Huesped",
                    CreadoPor = "Sistema",
                    NumeroUsos = 0,
                    FechaCreacion = DateTime.Now
                };

                await unitOfWork.CredencialesAcceso.AddAsync(nuevaCredencial, cancellationToken);
                _logger.LogInformation(
                    "Nueva credencial creada para HuespedId {HuespedId} (reserva {ReservaId})",
                    huespedId, evt.ReservaId);
            }
            else if (!credencialExistente.EstaActiva)
            {
                // Tenía credencial inactiva → reactivar
                credencialExistente.EstaActiva = true;
                await unitOfWork.CredencialesAcceso.UpdateAsync(credencialExistente, cancellationToken);
                _logger.LogInformation(
                    "Credencial {CredencialId} reactivada para HuespedId {HuespedId} (reserva {ReservaId})",
                    credencialExistente.CredencialId, huespedId, evt.ReservaId);
            }
            // Si ya tiene credencial activa → no hacer nada
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 3. Sincronizar ThingsBoard con el estado actualizado
        try
        {
            var syncService = scope.ServiceProvider.GetRequiredService<ITbCredencialesSyncService>();
            await syncService.SyncByReservaIdAsync(evt.ReservaId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sincronizando ThingsBoard para reserva {ReservaId}", evt.ReservaId);
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
