using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Dispositivos.Application.Interfaces;
using Dispositivos.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Domain.Events;

namespace Dispositivos.Infrastructure.Services;

public class CerraduraAccesoKafkaConsumerConfig
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string GroupId { get; set; } = "dispositivos-ceradura-acceso-group";
    public string Topic { get; set; } = "cerradura.acceso";
    public string AutoOffsetReset { get; set; } = "Earliest";
    public bool EnableAutoCommit { get; set; } = true;
    public int AutoCommitIntervalMs { get; set; } = 5000;
    public int SessionTimeoutMs { get; set; } = 30000;
    public int MaxPollIntervalMs { get; set; } = 300000;
}

public class CerraduraAccesoKafkaConsumer : BackgroundService
{
    private readonly CerraduraAccesoKafkaConsumerConfig _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CerraduraAccesoKafkaConsumer> _logger;
    private IConsumer<string, string>? _consumer;
    private IAdminClient? _adminClient;

    public CerraduraAccesoKafkaConsumer(
        CerraduraAccesoKafkaConsumerConfig config,
        IServiceScopeFactory scopeFactory,
        ILogger<CerraduraAccesoKafkaConsumer> logger)
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
        _logger.LogInformation("CerraduraAccesoKafkaConsumer started. Listening on topic: {Topic}", _config.Topic);

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
            _logger.LogInformation("CerraduraAccesoKafkaConsumer stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CerraduraAccesoKafkaConsumer");
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
            var evento = JsonSerializer.Deserialize<CerraduraAccesoEvent>(messageValue, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (evento == null)
            {
                _logger.LogWarning("Could not deserialize CerraduraAccesoEvent from message: {Message}", messageValue);
                return;
            }

            await HandleAccesoAsync(evento, cancellationToken);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing cerradura acceso message");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing cerradura acceso event");
        }
    }

    private async Task HandleAccesoAsync(CerraduraAccesoEvent evento, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing CerraduraAccesoEvent for dispositivo {DeviceName}, accessGranted: {AccessGranted}",
            evento.DeviceName, evento.Data?.AccessGranted);

        if (!Guid.TryParse(evento.DeviceName, out var dispositivoId))
        {
            _logger.LogWarning("DeviceName inválido: {DeviceName}", evento.DeviceName);
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var cerraduras = await unitOfWork.CerradurasInteligente.GetByDispositivoId(dispositivoId);
        var cerradura = cerraduras.FirstOrDefault(c => c.EstaActiva);

        if (cerradura == null)
        {
            _logger.LogWarning(
                "No se encontró cerradura activa para dispositivo {DispositivoId}",
                dispositivoId);
            return;
        }

        var credencialId = evento.Data?.CredId;
        var accessGranted = evento.Data?.AccessGranted ?? false;
        var accessMethod = evento.Data?.AccessMethod ?? "desconocido";

        var resultadoAcceso = accessGranted ? "Concedido" : "Denegado";
        var codigoError = accessGranted ? null : "ACCESS_DENIED";
        var motivoAcceso = accessGranted
            ? $"Acceso concedido via {accessMethod}"
            : $"Acceso denegado via {accessMethod}";

        var timestamp = evento.Timestamp > 0
            ? DateTimeOffset.FromUnixTimeMilliseconds(evento.Timestamp).UtcDateTime
            : DateTime.UtcNow;

        var registro = new RegistrosAcceso
        {
            CerraduraId = cerradura.CerraduraId,
            CredencialId = credencialId,
            UsuarioId = null,
            FechaHoraAcceso = timestamp,
            TipoAcceso = accessMethod,
            ResultadoAcceso = resultadoAcceso,
            MotivoAcceso = motivoAcceso,
            DireccionIp = null,
            InfoDispositivo = evento.DeviceName,
            FueExitoso = accessGranted,
            CodigoError = codigoError,
            Latencia = null
        };

        await unitOfWork.RegistrosAcceso.AddAsync(registro, cancellationToken);

        if (accessGranted && credencialId.HasValue)
        {
            cerradura.ContadorAperturas += 1;

            var credenciales = await unitOfWork.CredencialesAcceso.GetAll();
            var credencial = credenciales.FirstOrDefault(c => c.CredencialId == credencialId.Value);

            if (credencial != null)
            {
                credencial.NumeroUsos += 1;
                credencial.UltimoUso = DateTime.UtcNow;

                if (credencial.FechaExpiracion < DateTime.UtcNow && credencial.EstaActiva)
                {
                    credencial.EstaActiva = false;
                    _logger.LogInformation("Credencial {CredencialId} marcada como inactiva por expiración", credencialId);
                }
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "RegistrosAcceso creado para cerradura {CerraduraId}, acceso {Resultado}, credencial {CredencialId}",
            cerradura.CerraduraId, resultadoAcceso, credencialId);
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        _adminClient?.Dispose();
        base.Dispose();
    }
}