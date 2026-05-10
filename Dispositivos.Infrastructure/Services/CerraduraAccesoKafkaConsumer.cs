using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Dispositivos.Application.Interfaces;
using Dispositivos.Domain.Entities;
using Grpc.Net.Client;
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
    public string PersonalAccesoProducerTopic { get; set; } = "habitacion.personal-acceso";
    public string UsuariosGrpcUrl { get; set; } = "https://localhost:5285";
    public string AuthenticateGrpcUrl { get; set; } = "https://localhost:5118";
    public string ReservasGrpcUrl { get; set; } = "http://localhost:5142";
    public bool SkipCertValidation { get; set; }
}

public class CerraduraAccesoKafkaConsumer : BackgroundService
{
    private readonly CerraduraAccesoKafkaConsumerConfig _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CerraduraAccesoKafkaConsumer> _logger;
    private IConsumer<string, string>? _consumer;
    private IAdminClient? _adminClient;
    private IProducer<string, string>? _producer;

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

        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = _config.BootstrapServers,
            Acks = Acks.Leader
        }).Build();

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
            if (!metadata.Topics.Any(t => t.Topic == _config.PersonalAccesoProducerTopic))
            {
                _adminClient.CreateTopicsAsync(new[]
                {
                    new TopicSpecification { Name = _config.PersonalAccesoProducerTopic, NumPartitions = 1, ReplicationFactor = 1 }
                }).GetAwaiter().GetResult();

                _logger.LogInformation("Topic {Topic} created", _config.PersonalAccesoProducerTopic);
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
            "Processing CerraduraAccesoEvent for dispositivo {DeviceName}, accessGranted: {AccessGranted}, credTipo: {CredTipo}",
            evento.DeviceName, evento.Data?.AccessGranted, evento.Data?.CredTipo);

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
        int? credencialIdActual = null;
        var accessGranted = evento.Data?.AccessGranted ?? false;
        var accessMethod = evento.Data?.AccessMethod ?? "desconocido";
        var credTipo = evento.Data?.CredTipo ?? string.Empty;

        if (accessGranted && evento.Data?.CredPin.HasValue == true 
            && evento.Data.CredAct.HasValue == true 
            && evento.Data.CredExp.HasValue == true)
        {
            var pin = evento.Data.CredPin.Value.ToString();
            var fechaActivacion = evento.Data.CredAct.Value;
            var fechaExpiracion = evento.Data.CredExp.Value;

            _logger.LogInformation(
                "Buscando credencial por PIN {Pin}, Activacion {Activacion}, Expiracion {Expiracion}",
                pin, fechaActivacion, fechaExpiracion);

            var credencialRepo = unitOfWork.CredencialesAcceso;
            var credencialPorPin = await credencialRepo.GetByPinAndFechas(pin, fechaActivacion, fechaExpiracion);

            if (credencialPorPin != null)
            {
                _logger.LogInformation("Credencial {CredencialId} encontrada por PIN y fechas", credencialPorPin.CredencialId);
                credencialId = credencialPorPin.CredencialId;
                credencialIdActual = credencialPorPin.CredencialId;
                credTipo = credencialPorPin.TipoCredencial ?? credTipo;
                await ActualizarCredencialConId(unitOfWork, cerradura, credencialPorPin.CredencialId, cancellationToken);
            }
            else
            {
                _logger.LogWarning(
                    "No se encontró credencial para PIN {Pin} con las fechas especificadas",
                    pin);
            }
        }
        else if (accessGranted && credencialId.HasValue)
        {
            await ActualizarCredencialConId(unitOfWork, cerradura, credencialId.Value, cancellationToken);
        }

        var timestamp = evento.Timestamp > 0
            ? DateTimeOffset.FromUnixTimeMilliseconds(evento.Timestamp).UtcDateTime
            : DateTime.Now;

        var resultadoAcceso = accessGranted ? "Concedido" : "Denegado";
        var codigoError = accessGranted ? null : "ACCESS_DENIED";
        var motivoAcceso = accessGranted
            ? $"Acceso concedido via {accessMethod}"
            : $"Accceso denegado via {accessMethod}";

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

        await unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "RegistrosAcceso creado para cerradura {CerraduraId}, acceso {Resultado}, credencial {CredencialId}",
            cerradura.CerraduraId, resultadoAcceso, credencialId);

        _logger.LogInformation(
            "Evaluando notificacion: accessGranted={AccessGranted}, credTipo={CredTipo}, credencialId={CredencialId}",
            accessGranted, credTipo, credencialIdActual.HasValue ? credencialIdActual.Value : credencialId);

        if (accessGranted && string.Equals(credTipo, "personal", StringComparison.OrdinalIgnoreCase) && credencialIdActual.HasValue)
        {
            _logger.LogInformation("Ejecutando NotificarAccesoPersonalAsync para credencial {CredencialId}", credencialIdActual.Value);
            await NotificarAccesoPersonalAsync(cerradura, credencialIdActual.Value, timestamp, cancellationToken);
        }
    }

    private async Task NotificarAccesoPersonalAsync(
        CerradurasInteligente cerradura,
        int credencialId,
        DateTime fechaAcceso,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Procesando notificación de acceso personal para credencial {CredencialId}, cerradura {CerraduraId}",
                credencialId, cerradura.CerraduraId);

            var credencial = await GetCredencialConDatosAsync(credencialId, cancellationToken);
            if (credencial == null || !credencial.Value.PersonalId.HasValue)
            {
                _logger.LogWarning("Credencial {CredencialId} no tiene PersonalId asociado", credencialId);
                return;
            }

            var personalId = credencial.Value.PersonalId.Value;

            var datosPersonal = await ObtenerDatosPersonalAsync(personalId, cancellationToken);

            var huespedes = new List<HuespedCheckInInfo>();

            var reservaResponse = cerradura.HabitacionId.HasValue
                ? await ObtenerReservaActivaAsync(cerradura.HabitacionId.Value, cancellationToken)
                : null;
            if (reservaResponse != null)
            {
                _logger.LogInformation(
                    "Reserva activa {ReservaId} encontrada para habitacion {HabitacionId}",
                    reservaResponse.ReservaId, cerradura.HabitacionId);

                var huespedIds = new List<int>();
                if (reservaResponse.HuespedId > 0)
                    huespedIds.Add(reservaResponse.HuespedId);
                huespedIds.AddRange(reservaResponse.HuespedesAdicionales);

                foreach (var huespedId in huespedIds)
                {
                    var datosHuesped = await ObtenerDatosHuespedAsync(huespedId, cancellationToken);
                    if (datosHuesped != null && !string.IsNullOrEmpty(datosHuesped.Value.Email))
                    {
                        huespedes.Add(new HuespedCheckInInfo
                        {
                            HuespedId = huespedId,
                            Email = datosHuesped.Value.Email,
                            NombreCompleto = datosHuesped.Value.NombreCompleto
                        });
                        _logger.LogInformation("Datos obtenidos para huesped {HuespedId}: {Nombre}", huespedId, datosHuesped.Value.NombreCompleto);
                    }
                }
            }
            else
            {
                _logger.LogWarning("No se encontró reserva activa para habitacion {HabitacionId}", cerradura.HabitacionId);
            }

            var accesoEvent = new PersonalAccesoHabitacionEvent
            {
                HabitacionId = cerradura.HabitacionId ?? 0,
                NumeroHabitacion = reservaResponse?.NumeroHabitacion ?? cerradura.HabitacionId?.ToString() ?? string.Empty,
                PersonalId = personalId,
                NombrePersonal = datosPersonal?.NombreCompleto ?? $"Personal {personalId}",
                Puesto = datosPersonal?.Cargo,
                Departamento = datosPersonal?.Departamento,
                EsAccesoFisico = true,
                Huespedes = huespedes,
                FechaAcceso = DateTime.SpecifyKind(
                    fechaAcceso.Kind == DateTimeKind.Utc ? fechaAcceso.ToLocalTime() : fechaAcceso,
                    DateTimeKind.Unspecified)
            };

            var message = new Message<string, string>
            {
                Key = cerradura.HabitacionId?.ToString() ?? string.Empty,
                Value = JsonSerializer.Serialize(accesoEvent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            };

            await _producer!.ProduceAsync(_config.PersonalAccesoProducerTopic, message, cancellationToken);

            _logger.LogInformation(
                "PersonalAccesoHabitacionEvent publicado para habitacion {HabitacionId}, {Count} huespedes, acceso físico",
                cerradura.HabitacionId, huespedes.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al notificar acceso personal para credencial {CredencialId}", credencialId);
        }
    }

    private async Task<Grpc.Contracts.Reservas.ReservaResponse?> ObtenerReservaActivaAsync(
        int habitacionId, CancellationToken cancellationToken)
    {
        var grpcUrl = _config.ReservasGrpcUrl;
        _logger.LogInformation("[Notificacion] Obteniendo reserva activa para habitacion {HabitacionId} desde {GrpcUrl}", habitacionId, grpcUrl);

        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                var httpHandler = new System.Net.Http.HttpClientHandler();
                if (_config.SkipCertValidation)
                {
                    httpHandler.ServerCertificateCustomValidationCallback =
                        System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }

                using var channel = GrpcChannel.ForAddress(grpcUrl, new GrpcChannelOptions
                {
                    HttpHandler = httpHandler,
                    DisposeHttpClient = true
                });
                var client = new Grpc.Contracts.Reservas.Reserva.ReservaClient(channel);
                var response = await client.GetReservaActivaByHabitacionIdAsync(
                    new Grpc.Contracts.Reservas.GetReservaActivaByHabitacionIdRequest { HabitacionId = habitacionId },
                    cancellationToken: cancellationToken);

                if (response.Found)
                {
                    _logger.LogInformation(
                        "[Notificacion] Reserva {ReservaId} encontrada para habitacion {HabitacionId}",
                        response.ReservaId, habitacionId);
                    return response;
                }
                _logger.LogWarning("[Notificacion] No se encontró reserva activa para habitacion {HabitacionId}", habitacionId);
                return null;
            }
            catch (Exception ex) when (attempt < 3)
            {
                _logger.LogWarning(ex, "[Notificacion] Error gRPC Reserva (attempt {Attempt}/3): {Message}", attempt, ex.Message);
                await Task.Delay(500 * attempt, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Notificacion] Error gRPC Reserva final: {Message}", ex.Message);
            }
        }
        return null;
    }

    private async Task<(string Email, string NombreCompleto)?> ObtenerDatosHuespedAsync(int huespedId, CancellationToken cancellationToken)
    {
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                var httpHandler = new System.Net.Http.HttpClientHandler();
                if (_config.SkipCertValidation)
                    httpHandler.ServerCertificateCustomValidationCallback =
                        System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                using var channel = GrpcChannel.ForAddress(_config.UsuariosGrpcUrl, new GrpcChannelOptions
                {
                    HttpHandler = httpHandler,
                    DisposeHttpClient = true
                });
                var huespedResponse = await new Grpc.Contracts.Usuarios.Huesped.HuespedClient(channel)
                    .GetHuespedByIdAsync(
                        new Grpc.Contracts.Usuarios.GetHuespedByIdRequest { HuespedId = huespedId },
                        cancellationToken: cancellationToken);

                if (!huespedResponse.Found || string.IsNullOrEmpty(huespedResponse.UsuarioId))
                {
                    _logger.LogWarning("[Notificacion] Huesped {HuespedId} no encontrado en Usuarios API", huespedId);
                    return null;
                }

                var httpHandler2 = new System.Net.Http.HttpClientHandler();
                if (_config.SkipCertValidation)
                    httpHandler2.ServerCertificateCustomValidationCallback =
                        System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                using var channel2 = GrpcChannel.ForAddress(_config.AuthenticateGrpcUrl, new GrpcChannelOptions
                {
                    HttpHandler = httpHandler2,
                    DisposeHttpClient = true
                });
                var authResponse = await new Grpc.Contracts.Authentication.UserLookup.UserLookupClient(channel2)
                    .GetEmailByUserIdAsync(
                        new Grpc.Contracts.Authentication.GetEmailByUserIdRequest { UserId = huespedResponse.UsuarioId },
                        cancellationToken: cancellationToken);

                if (!authResponse.Found)
                {
                    _logger.LogWarning("[Notificacion] Usuario {UsuarioId} no encontrado en Auth API", huespedResponse.UsuarioId);
                    return null;
                }

                var nombre = !string.IsNullOrEmpty(huespedResponse.NombreCompleto)
                    ? huespedResponse.NombreCompleto
                    : $"Huésped {huespedId}";

                _logger.LogInformation("[Notificacion] Datos obtenidos para huesped {HuespedId}: {Nombre}, {Email}", huespedId, nombre, authResponse.Email);
                return (authResponse.Email, nombre);
            }
            catch (Exception ex) when (attempt < 3)
            {
                _logger.LogWarning(ex, "[Notificacion] Error gRPC huesped (attempt {Attempt}/3): {Message}", attempt, ex.Message);
                await Task.Delay(500 * attempt, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Notificacion] Error gRPC final para huesped {HuespedId}: {Message}", huespedId, ex.Message);
            }
        }
        return null;
    }

    private async Task<(string NombreCompleto, string? Cargo, string? Departamento)?> ObtenerDatosPersonalAsync(int personalId, CancellationToken cancellationToken)
    {
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                var httpHandler = new System.Net.Http.HttpClientHandler();
                if (_config.SkipCertValidation)
                    httpHandler.ServerCertificateCustomValidationCallback =
                        System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                using var channel = GrpcChannel.ForAddress(_config.UsuariosGrpcUrl, new GrpcChannelOptions
                {
                    HttpHandler = httpHandler,
                    DisposeHttpClient = true
                });
                var response = await new Grpc.Contracts.Usuarios.Personal.PersonalClient(channel)
                    .GetPersonalByIdAsync(
                        new Grpc.Contracts.Usuarios.GetPersonalByIdRequest { PersonalId = personalId },
                        cancellationToken: cancellationToken);

                if (!response.Found)
                {
                    _logger.LogWarning("[Notificacion] Personal {PersonalId} no encontrado en Usuarios API", personalId);
                    return null;
                }

                var nombre = !string.IsNullOrEmpty(response.NombreCompleto)
                    ? response.NombreCompleto
                    : $"Personal {personalId}";

                _logger.LogInformation("[Notificacion] Datos personal {PersonalId}: {Nombre}, {Cargo}, {Departamento}",
                    personalId, nombre, response.Cargo, response.Departamento);

                return (nombre,
                    string.IsNullOrEmpty(response.Cargo) ? null : response.Cargo,
                    string.IsNullOrEmpty(response.Departamento) ? null : response.Departamento);
            }
            catch (Exception ex) when (attempt < 3)
            {
                _logger.LogWarning(ex, "[Notificacion] Error gRPC personal (attempt {Attempt}/3): {Message}", attempt, ex.Message);
                await Task.Delay(500 * attempt, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Notificacion] Error gRPC final para personal {PersonalId}: {Message}", personalId, ex.Message);
            }
        }
        return null;
    }

    private async Task<(int? PersonalId, int? HuespedId, int? ReservaId)?> GetCredencialConDatosAsync(
        int credencialId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var credencial = await unitOfWork.CredencialesAcceso.GetById(credencialId);

        if (credencial == null) return null;

        return (credencial.PersonalId, credencial.HuespedId, credencial.ReservaId);
}

    private async Task ActualizarCredencialConId(IUnitOfWork unitOfWork, CerradurasInteligente cerradura, int credId, CancellationToken cancellationToken)
    {
        cerradura.ContadorAperturas += 1;
        cerradura.UltimaApertura = DateTime.Now;
        await unitOfWork.CerradurasInteligente.UpdateAsync(cerradura, cancellationToken);

        var credencialRepo = unitOfWork.CredencialesAcceso;
        var credencial = await credencialRepo.GetById(credId);

        if (credencial != null)
        {
            credencial.NumeroUsos += 1;
            credencial.UltimoUso = DateTime.Now;

            if (credencial.FechaExpiracion < DateTime.Now && credencial.EstaActiva)
            {
                credencial.EstaActiva = false;
                _logger.LogInformation("Credencial {CredencialId} marcada como inactiva por expiración", credId);
            }

            await credencialRepo.UpdateAsync(credencial, cancellationToken);
        }
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        _adminClient?.Dispose();
        _producer?.Flush(TimeSpan.FromSeconds(5));
        _producer?.Dispose();
        base.Dispose();
    }
}