using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;
using Notification.Domain.Interfaces;
using System.Security.Claims;

namespace Dispositivos.Application.Behaviors;

public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAuditProducer _auditProducer;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditBehavior<TRequest, TResponse>> _logger;

    private const string ServiceName = "Dispositivos.API";

    public AuditBehavior(
        IAuditProducer auditProducer,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditBehavior<TRequest, TResponse>> logger)
    {
        _auditProducer = auditProducer;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var commandName = typeof(TRequest).Name;

        if (!commandName.EndsWith("Command"))
            return await next();

        TResponse response;
        bool isSuccess;
        string? errorMessage = null;

        try
        {
            response = await next();
            isSuccess = GetIsSuccess(response, out errorMessage);
        }
        catch (Exception ex)
        {
            await PublishAuditAsync(commandName, false, ex.Message, cancellationToken, request);
            throw;
        }

        await PublishAuditAsync(commandName, isSuccess, errorMessage, cancellationToken, request);
        return response;
    }

    private async Task PublishAuditAsync(string commandName, bool isSuccess, string? errorMessage, CancellationToken ct, TRequest? request = default)
    {
        try
        {
            var (accion, tipoEntidad) = ParseCommandName(commandName);
            var ctx = _httpContextAccessor.HttpContext;
            var userId = ctx?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var auditEvent = new AuditEvent
            {
                Servicio = ServiceName,
                UsuarioId = string.IsNullOrWhiteSpace(userId) ? null : userId,
                Accion = accion,
                TipoEntidad = tipoEntidad,
                EntidadId = request is not null ? ExtractEntidadId(request) : null,
                ValorNuevo = request is not null ? JsonSerializer.Serialize(request) : null,
                DireccionIp = ctx?.Connection?.RemoteIpAddress?.ToString() ?? string.Empty,
                AgenteUsuario = ctx?.Request?.Headers["User-Agent"].ToString(),
                Resultado = isSuccess ? "Exitoso" : "Fallido",
                MensajeError = errorMessage,
                FechaHora = DateTime.UtcNow
            };

            await _auditProducer.PublishAsync(auditEvent, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error publicando evento de auditoría para {Command}", commandName);
        }
    }

    private static int? ExtractEntidadId(TRequest request)
    {
        var idProp = request.GetType()
            .GetProperties()
            .FirstOrDefault(p =>
                p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) &&
                (p.PropertyType == typeof(int) || p.PropertyType == typeof(int?)));

        return idProp?.GetValue(request) as int?;
    }

    private static bool GetIsSuccess(TResponse response, out string? errorMessage)
    {
        errorMessage = null;
        if (response is null) return true;

        var type = response.GetType();
        var isSuccessProp = type.GetProperty("IsSuccess");
        if (isSuccessProp is null) return true;

        var isSuccess = (bool)(isSuccessProp.GetValue(response) ?? true);
        if (!isSuccess)
        {
            var errorProp = type.GetProperty("ErrorMessage");
            errorMessage = errorProp?.GetValue(response) as string;
        }
        return isSuccess;
    }

    private static (string accion, string tipoEntidad) ParseCommandName(string commandName)
    {
        var body = commandName.Replace("Command", string.Empty);

        var knownActions = new[]
        {
            ("CreateRegistrosAcceso", "ACCESS", "RegistroAcceso"),
            ("CreateMantenimientoCerradura", "MAINTENANCE", "Cerradura"),
            ("Create", "CREATE", ""),
            ("Update", "UPDATE", ""),
            ("Delete", "DELETE", "")
        };

        foreach (var (prefix, action, overrideEntity) in knownActions)
        {
            if (body.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var entity = !string.IsNullOrEmpty(overrideEntity)
                    ? overrideEntity
                    : body[prefix.Length..];

                return (action, string.IsNullOrEmpty(entity) ? "Unknown" : entity);
            }
        }

        return ("ACTION", body);
    }
}
