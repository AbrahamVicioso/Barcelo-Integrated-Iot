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

public class CheckInRealizadoKafkaConsumerConfig
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string GroupId { get; set; } = "dispositivos-checkin-group";
    public string Topic { get; set; } = "reservas.checkin-realizado";
    public string AutoOffsetReset { get; set; } = "Earliest";
    public bool EnableAutoCommit { get; set; } = true;
    public int AutoCommitIntervalMs { get; set; } = 5000;
    public int SessionTimeoutMs { get; set; } = 30000;
    public int MaxPollIntervalMs { get; set; } = 300000;
    public string CredencialesProducerTopic { get; set; } = "checkin.credenciales";
}

public class CheckInRealizadoKafkaConsumer : BackgroundService
{
    private readonly CheckInRealizadoKafkaConsumerConfig _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CheckInRealizadoKafkaConsumer> _logger;
    private IConsumer<string, string>? _consumer;
    private IAdminClient? _adminClient;
    private IProducer<string, string>? _producer;

    public CheckInRealizadoKafkaConsumer(
        CheckInRealizadoKafkaConsumerConfig config,
        IServiceScopeFactory scopeFactory,
        ILogger<CheckInRealizadoKafkaConsumer> logger)
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
        _logger.LogInformation("CheckInRealizadoKafkaConsumer started. Listening on topic: {Topic}", _config.Topic);

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
            _logger.LogInformation("CheckInRealizadoKafkaConsumer stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CheckInRealizadoKafkaConsumer");
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
            var checkInEvent = JsonSerializer.Deserialize<CheckInRealizadoEvent>(messageValue, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (checkInEvent == null)
            {
                _logger.LogWarning("Could not deserialize CheckInRealizadoEvent: {Message}", messageValue);
                return;
            }

            await HandleCheckInRealizadoAsync(checkInEvent, cancellationToken);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing CheckInRealizadoEvent message");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CheckInRealizadoEvent");
        }
    }

    private async Task HandleCheckInRealizadoAsync(CheckInRealizadoEvent checkInEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Procesando CheckInRealizadoEvent para reserva {NumeroReserva}, {Count} huespedes",
            checkInEvent.NumeroReserva, checkInEvent.Huespedes.Count);

        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var credencialesGeneradas = new List<CredencialHuespedInfo>();

        foreach (var huesped in checkInEvent.Huespedes)
        {
            try
            {
                var pin = GenerarPin();
                var credencial = new Dispositivos.Domain.Entities.CredencialesAcceso
                {
                    HuespedId = huesped.HuespedId,
                    ReservaId = checkInEvent.ReservaId,
                    CodigoPin = pin,
                    HashPin = GenerarHash(pin),
                    FechaActivacion = checkInEvent.FechaCheckIn,
                    FechaExpiracion = checkInEvent.FechaCheckOut,
                    EstaActiva = true,
                    TipoCredencial = "Huesped",
                    CreadoPor = "Sistema",
                    NumeroUsos = 0,
                    FechaCreacion = DateTime.UtcNow
                };

                await unitOfWork.CredencialesAcceso.AddAsync(credencial, cancellationToken);

                credencialesGeneradas.Add(new CredencialHuespedInfo
                {
                    HuespedId = huesped.HuespedId,
                    Email = huesped.Email,
                    NombreCompleto = huesped.NombreCompleto,
                    CodigoPin = pin
                });

                _logger.LogInformation(
                    "Credencial generada para HuespedId {HuespedId}, reserva {NumeroReserva}",
                    huesped.HuespedId, checkInEvent.NumeroReserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al crear credencial para HuespedId {HuespedId}, reserva {NumeroReserva}",
                    huesped.HuespedId, checkInEvent.NumeroReserva);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Publicar evento para que Notification.Worker envíe el PIN por email a cada huésped
        if (credencialesGeneradas.Count > 0)
            await PublicarCredencialesEventAsync(checkInEvent, credencialesGeneradas, cancellationToken);
    }

    private async Task PublicarCredencialesEventAsync(
        CheckInRealizadoEvent checkInEvent,
        List<CredencialHuespedInfo> credenciales,
        CancellationToken cancellationToken)
    {
        try
        {
            var notifEvent = new CredencialesCheckInEvent
            {
                ReservaId = checkInEvent.ReservaId,
                NumeroReserva = checkInEvent.NumeroReserva,
                FechaCheckIn = checkInEvent.FechaCheckIn,
                FechaCheckOut = checkInEvent.FechaCheckOut,
                Credenciales = credenciales
            };

            var message = new Message<string, string>
            {
                Key = checkInEvent.NumeroReserva,
                Value = JsonSerializer.Serialize(notifEvent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            };

            await _producer!.ProduceAsync(_config.CredencialesProducerTopic, message, cancellationToken);

            _logger.LogInformation(
                "CredencialesCheckInEvent publicado para reserva {NumeroReserva}, {Count} huespedes",
                checkInEvent.NumeroReserva, credenciales.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publicando CredencialesCheckInEvent para reserva {NumeroReserva}",
                checkInEvent.NumeroReserva);
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
        _producer?.Flush(TimeSpan.FromSeconds(5));
        _producer?.Dispose();
        _consumer?.Dispose();
        _adminClient?.Dispose();
        base.Dispose();
    }
}
