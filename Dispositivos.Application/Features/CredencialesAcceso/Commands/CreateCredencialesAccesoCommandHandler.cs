using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Dispositivos.Application.Common;
using Dispositivos.Application.DTOs;
using Dispositivos.Application.Interfaces;
using CredencialEntity = Dispositivos.Domain.Entities.CredencialesAcceso;
using Grpc.Net.Client;
using Notification.Domain.Events;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Dispositivos.Application.Features.CredencialesAcceso.Commands;

public class CreateCredencialesAccesoCommandHandler : IRequestHandler<CreateCredencialesAccesoCommand, Result<int>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICredencialesAccesoRepository _credencialRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CreateCredencialesAccesoCommandHandler> _logger;

    public CreateCredencialesAccesoCommandHandler(
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ICredencialesAccesoRepository credencialRepository,
        IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<CreateCredencialesAccesoCommandHandler> logger)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _credencialRepository = credencialRepository;
        _httpContextAccessor = httpContextAccessor;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    private string GenerarHash(string texto)
{
    using var sha256 = SHA256.Create();
    var bytes = Encoding.UTF8.GetBytes(texto);
    var hash = sha256.ComputeHash(bytes);
    return Convert.ToBase64String(hash);
}

    private async Task<string?> ObtenerUsuarioIdDeHuespedAsync(int huespedId, CancellationToken cancellationToken)
    {
        var grpcUrl = _configuration["ExternalServices:Usuarios:GrpcUrl"] ?? "http://localhost:5285";
        var skipCertValidation = _configuration.GetValue<bool>("ExternalServices:Usuarios:SkipCertValidation");
        _logger.LogInformation("[Credenciales] Obteniendo UsuarioId de Huesped {HuespedId} desde {GrpcUrl}", huespedId, grpcUrl);

        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                var httpHandler = new System.Net.Http.HttpClientHandler();
                if (skipCertValidation)
                {
                    httpHandler.ServerCertificateCustomValidationCallback =
                        System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }

                using var channel = GrpcChannel.ForAddress(grpcUrl, new GrpcChannelOptions
                {
                    HttpHandler = httpHandler,
                    DisposeHttpClient = true
                });
                var client = new Grpc.Contracts.Usuarios.Huesped.HuespedClient(channel);
                var response = await client.GetHuespedByIdAsync(new Grpc.Contracts.Usuarios.GetHuespedByIdRequest { HuespedId = huespedId }, cancellationToken: cancellationToken);
                if (response.Found)
                {
                    _logger.LogInformation("[Credenciales] UsuarioId obtenido: {UsuarioId} para Huesped {HuespedId}", response.UsuarioId, huespedId);
                    return response.UsuarioId;
                }
                _logger.LogWarning("[Credenciales] Huesped {HuespedId} no encontrado en Users API", huespedId);
                return null;
            }
            catch (Exception ex) when (attempt < 3)
            {
                _logger.LogWarning(ex, "[Credenciales] Error gRPC Huesped (attempt {Attempt}/3): {Message}", attempt, ex.Message);
                await Task.Delay(500 * attempt, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Credenciales] Error gRPC Huesped final: {Message}", ex.Message);
            }
        }
        return null;
    }

    private async Task<string?> ObtenerUsuarioIdDePersonalAsync(int personalId, CancellationToken cancellationToken)
    {
        var grpcUrl = _configuration["ExternalServices:Usuarios:GrpcUrl"] ?? "http://localhost:5285";
        var skipCertValidation = _configuration.GetValue<bool>("ExternalServices:Usuarios:SkipCertValidation");
        _logger.LogInformation("[Credenciales] Obteniendo UsuarioId de Personal {PersonalId} desde {GrpcUrl}", personalId, grpcUrl);

        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                var httpHandler = new System.Net.Http.HttpClientHandler();
                if (skipCertValidation)
                {
                    httpHandler.ServerCertificateCustomValidationCallback =
                        System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }

                using var channel = GrpcChannel.ForAddress(grpcUrl, new GrpcChannelOptions
                {
                    HttpHandler = httpHandler,
                    DisposeHttpClient = true
                });
                var client = new Grpc.Contracts.Usuarios.Personal.PersonalClient(channel);
                var response = await client.GetPersonalByIdAsync(new Grpc.Contracts.Usuarios.GetPersonalByIdRequest { PersonalId = personalId }, cancellationToken: cancellationToken);
                if (response.Found)
                {
                    _logger.LogInformation("[Credenciales] UsuarioId obtenido: {UsuarioId} para Personal {PersonalId}", response.UsuarioId, personalId);
                    return response.UsuarioId;
                }
                _logger.LogWarning("[Credenciales] Personal {PersonalId} no encontrado en Users API", personalId);
                return null;
            }
            catch (Exception ex) when (attempt < 3)
            {
                _logger.LogWarning(ex, "[Credenciales] Error gRPC Personal (attempt {Attempt}/3): {Message}", attempt, ex.Message);
                await Task.Delay(500 * attempt, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Credenciales] Error gRPC Personal final: {Message}", ex.Message);
            }
        }
        return null;
    }

    private async Task<string?> ObtenerEmailDeAuthAsync(string usuarioId, CancellationToken cancellationToken)
    {
        var grpcUrl = _configuration["ExternalServices:Authenticate:GrpcUrl"] ?? "http://localhost:5118";
        var skipCertValidation = _configuration.GetValue<bool>("ExternalServices:Authenticate:SkipCertValidation");
        _logger.LogInformation("[Credenciales] Obteniendo Email de Usuario {UsuarioId} desde {GrpcUrl}", usuarioId, grpcUrl);

        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                var httpHandler = new System.Net.Http.HttpClientHandler();
                if (skipCertValidation)
                {
                    httpHandler.ServerCertificateCustomValidationCallback =
                        System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }

                using var channel = GrpcChannel.ForAddress(grpcUrl, new GrpcChannelOptions
                {
                    HttpHandler = httpHandler,
                    DisposeHttpClient = true
                });
                var client = new Grpc.Contracts.Authentication.UserLookup.UserLookupClient(channel);
                var response = await client.GetEmailByUserIdAsync(new Grpc.Contracts.Authentication.GetEmailByUserIdRequest { UserId = usuarioId }, cancellationToken: cancellationToken);
                if (response.Found)
                {
                    _logger.LogInformation("[Credenciales] Email obtenido: {Email} para Usuario {UsuarioId}", response.Email, usuarioId);
                    return response.Email;
                }
                _logger.LogWarning("[Credenciales] Usuario {UsuarioId} no encontrado en Auth API", usuarioId);
                return null;
            }
            catch (Exception ex) when (attempt < 3)
            {
                _logger.LogWarning(ex, "[Credenciales] Error gRPC Auth (attempt {Attempt}/3): {Message}", attempt, ex.Message);
                await Task.Delay(500 * attempt, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Credenciales] Error gRPC Auth final: {Message}", ex.Message);
            }
        }
        return null;
    }

    public async Task<Result<int>> Handle(CreateCredencialesAccesoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Credencial.FechaExpiracion <= request.Credencial.FechaActivacion)
                return Result<int>.Failure("La fecha de expiración debe ser posterior a la fecha de activación.");

            if (request.Credencial.FechaActivacion < DateTime.Now.Date)
                return Result<int>.Failure("La fecha de activación no puede ser en el pasado.");

            var user = _httpContextAccessor.HttpContext?.User;
            var userId = user?.FindFirst("nameid")?.Value
                      ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Result<int>.Failure("No se pudo identificar al usuario autenticado.");

            var credencial = _mapper.Map<CredencialEntity>(request.Credencial);
            credencial.FechaCreacion = DateTime.Now;
            credencial.CreadoPor = userId;
            credencial.HashPin = GenerarHash(credencial.CodigoPin);

            await _credencialRepository.AddAsync(credencial, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Sync ThingsBoard: todas las cerraduras del usuario afectado
            using var scope = _scopeFactory.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<ITbCredencialesSyncService>();
            if (request.Credencial.PersonalId.HasValue)
                await syncService.SyncByPersonalIdAsync(request.Credencial.PersonalId.Value, cancellationToken);
            else if (request.Credencial.HuespedId.HasValue && request.Credencial.ReservaActividadId.HasValue)
                await syncService.SyncByReservaActividadIdAsync(request.Credencial.ReservaActividadId.Value, cancellationToken);
            else if (request.Credencial.HuespedId.HasValue)
                await syncService.SyncByHuespedIdAsync(request.Credencial.HuespedId.Value, cancellationToken);
            else if (request.Credencial.ReservaId.HasValue)
                await syncService.SyncByReservaIdAsync(request.Credencial.ReservaId.Value, cancellationToken);
            else if (request.Credencial.ReservaActividadId.HasValue)
                await syncService.SyncByReservaActividadIdAsync(request.Credencial.ReservaActividadId.Value, cancellationToken);

            // Publicar evento para enviar notificación por email y push
            try
            {
                var kafkaProducer = scope.ServiceProvider.GetRequiredService<ICredencialesKafkaProducer>();
                var credencialEvent = new CredencialCreadaEvent
                {
                    CredencialId = credencial.CredencialId,
                    HuespedId = request.Credencial.HuespedId,
                    PersonalId = request.Credencial.PersonalId,
                    ReservaId = request.Credencial.ReservaId,
                    CodigoPin = credencial.CodigoPin,
                    FechaActivacion = credencial.FechaActivacion,
                    FechaExpiracion = credencial.FechaExpiracion,
                    TipoCredencial = credencial.TipoCredencial
                };

                // Obtener UsuarioId desde Usuarios.API y luego Email desde Authenticate.API
                string? usuarioId = null;

                if (request.Credencial.HuespedId.HasValue)
                {
                    _logger.LogInformation("[Credenciales] Iniciando obtención de email para HuespedId {HuespedId}", request.Credencial.HuespedId.Value);
                    usuarioId = await ObtenerUsuarioIdDeHuespedAsync(request.Credencial.HuespedId.Value, cancellationToken);
                }
                else if (request.Credencial.PersonalId.HasValue)
                {
                    _logger.LogInformation("[Credenciales] Iniciando obtención de email para PersonalId {PersonalId}", request.Credencial.PersonalId.Value);
                    usuarioId = await ObtenerUsuarioIdDePersonalAsync(request.Credencial.PersonalId.Value, cancellationToken);
                }

                string? email = null;
                if (!string.IsNullOrEmpty(usuarioId))
                {
                    _logger.LogInformation("[Credenciales] UsuarioId obtenido: {UsuarioId}, obteniendo email...", usuarioId);
                    email = await ObtenerEmailDeAuthAsync(usuarioId, cancellationToken);
                }
                else
                {
                    _logger.LogWarning("[Credenciales] No se pudo obtener UsuarioId, omitiendo obtención de email");
                }

                credencialEvent.Email = email;
                credencialEvent.NombreCompleto = null; // El nombre no es necesario para la notificación

                if (!string.IsNullOrEmpty(email))
                {
                    _logger.LogInformation("[Credenciales] Email obtenido: {Email}, publicando evento a Kafka", email);
                }
                else
                {
                    _logger.LogWarning("[Credenciales] Email no obtenido, evento se publicará sin email");
                }

                await kafkaProducer.PublishCredencialCreadaAsync(credencialEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                // No fallar la operación si falla el envío de notificación
                _logger.LogError(ex, "[Credenciales] Error publicando evento de credencial: {Message}", ex.Message);
            }

            return Result<int>.Success(credencial.CredencialId);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateException")
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            if (inner.Contains("CHK_Credenciales_Fechas"))
                return Result<int>.Failure("La fecha de expiración debe ser posterior a la fecha de activación.");
            return Result<int>.Failure($"Error de base de datos al crear la credencial: {inner}");
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Error al crear la credencial de acceso: {ex.Message}");
        }
    }
}
