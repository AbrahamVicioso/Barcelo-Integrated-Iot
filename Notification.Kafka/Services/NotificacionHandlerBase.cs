using Microsoft.Extensions.Logging;
using Notification.Domain.Data;
using Notification.Domain.Entities;
using Notification.Domain.Helpers;
using Notification.Domain.Interfaces;

namespace Notification.Kafka.Services;

/// <summary>
/// Clase base para manejar notificaciones con verificación de preferencias
/// </summary>
public abstract class NotificacionHandlerBase
{
    private readonly IPreferenciasRepository _preferenciasRepo;
    private readonly INotificacionesRepository _notificacionesRepo;
    private readonly AuthApiClient _authApiClient;
    private readonly IEmailService _emailService;
    private readonly IPushNotificationService _pushService;
    private readonly ILogger _logger;

    protected NotificacionHandlerBase(
        IPreferenciasRepository preferenciasRepo,
        INotificacionesRepository notificacionesRepo,
        AuthApiClient authApiClient,
        IEmailService emailService,
        IPushNotificationService pushService,
        ILogger logger)
    {
        _preferenciasRepo = preferenciasRepo;
        _notificacionesRepo = notificacionesRepo;
        _authApiClient = authApiClient;
        _emailService = emailService;
        _pushService = pushService;
        _logger = logger;
    }

    /// <summary>
    /// Verifica si se debe enviar una notificación según las preferencias del usuario
    /// </summary>
    protected async Task<bool> DeboEnviarNotificacionAsync(
        string email,
        string canal,
        string tipoNotificacion,
        CancellationToken cancellationToken = default)
    {
        // 1. Obtener UsuarioId por email
        var usuarioId = await _authApiClient.GetUserIdByEmailAsync(email, cancellationToken);
        if (string.IsNullOrEmpty(usuarioId))
        {
            _logger.LogWarning("No se pudo obtener UsuarioId para email {Email}, enviando por defecto", email);
            return true;
        }

        // 2. Obtener preferencias
        var prefs = await _preferenciasRepo.GetByUsuarioIdAsync(usuarioId, cancellationToken);

        // 3. Si NO tiene preferencias → ENVIAR (defaults: todos true)
        if (prefs == null)
        {
            _logger.LogDebug("Usuario {UsuarioId} no tiene preferencias, usando defaults (enviar)", usuarioId);
            return true;
        }

        // 4. Verificar horario no molestar
        if (prefs.HorarioNoMolestar && EnHorarioNoMolestar(prefs))
        {
            _logger.LogDebug("Notificación omitida por horario no molestar para usuario {UsuarioId}", usuarioId);
            return false;
        }

        // 5. Verificar canal
        if (canal == "Push" && !prefs.HabilitarNotificacionesPush)
        {
            _logger.LogDebug("Notificación Push deshabilitada para usuario {UsuarioId}", usuarioId);
            return false;
        }

        if (canal == "Email" && !prefs.HabilitarNotificacionesEmail)
        {
            _logger.LogDebug("Notificación Email deshabilitada para usuario {UsuarioId}", usuarioId);
            return false;
        }

        if (canal == "SMS" && !prefs.HabilitarNotificacionesSMS)
        {
            _logger.LogDebug("Notificación SMS deshabilitada para usuario {UsuarioId}", usuarioId);
            return false;
        }

        // 6. Verificar tipo de notificación
        var debeEnviar = tipoNotificacion switch
        {
            "AccesoPersonal" => prefs.NotificarAccesoPersonal,
            "Reserva" => prefs.NotificarReservas,
            "Credencial" => prefs.NotificarCredenciales,
            "CheckIn" => prefs.NotificarCheckIn,
            "CuentaCreada" => prefs.NotificarCuentaCreada,
            "ConfirmacionEmail" => prefs.NotificarConfirmacionEmail,
            "RestablecerPassword" => prefs.NotificarRestablecerPassword,
            "RecordatorioActividad" => prefs.NotificarRecordatorioActividad,
            "PromocionesOfertas" => prefs.NotificarPromocionesOfertas,
            _ => true // Tipos no reconocidos se envían por defecto
        };

        if (!debeEnviar)
        {
            _logger.LogDebug("Notificación tipo {TipoNotificacion} deshabilitada para usuario {UsuarioId}", tipoNotificacion, usuarioId);
        }

        return debeEnviar;
    }

    /// <summary>
    /// Verifica si la hora actual está en el horario de no molestar
    /// </summary>
    private bool EnHorarioNoMolestar(PreferenciaNotificacion prefs)
    {
        var ahora = DateTime.UtcNow.TimeOfDay;
        var inicio = prefs.HoraInicioNoMolestar;
        var fin = prefs.HoraFinNoMolestar;

        if (inicio == null || fin == null) return false;

        // Caso normal: inicio < fin (ej: 09:00 a 18:00)
        if (inicio < fin)
            return ahora >= inicio && ahora < fin;

        // Caso noche: inicio > fin (ej: 22:00 a 08:00)
        return ahora >= inicio || ahora < fin;
    }

    /// <summary>
    /// Guarda la notificación en el historial
    /// </summary>
    protected async Task GuardarNotificacionAsync(
        string email,
        string tipoNotificacion,
        string titulo,
        string mensaje,
        string canal,
        string prioridad,
        string? tipoEntidad = null,
        int? entidadId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var usuarioId = await _authApiClient.GetUserIdByEmailAsync(email, cancellationToken);
            if (string.IsNullOrEmpty(usuarioId))
            {
                _logger.LogWarning("No se pudo obtener UsuarioId para guardar notificación con email {Email}", email);
                return;
            }

            var notificacion = new NotificacionEntity
            {
                UsuarioId = usuarioId,
                TipoNotificacion = tipoNotificacion.Length > 50 ? tipoNotificacion[..50] : tipoNotificacion,
                Titulo = titulo.Length > 200 ? titulo[..200] : titulo,
                Mensaje = mensaje,
                Prioridad = prioridad,
                FueLeida = false,
                FechaEnvio = DateTime.UtcNow,
                CanalEnvio = canal,
                EstadoEnvio = "Enviada",
                TipoEntidadRelacionada = tipoEntidad,
                EntidadRelacionadaId = entidadId
            };

            await _notificacionesRepo.AddAsync(notificacion, cancellationToken);
            _logger.LogDebug("Notificación guardada en historial para usuario {UsuarioId}", usuarioId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando notificación en historial para email {Email}", email);
        }
    }

    /// <summary>
    /// Envía una notificación por email verificando preferencias
    /// </summary>
    protected async Task<bool> EnviarEmailAsync(
        string email,
        string tipoNotificacion,
        EmailNotification emailNotification,
        CancellationToken cancellationToken = default)
    {
        if (!await DeboEnviarNotificacionAsync(email, "Email", tipoNotificacion, cancellationToken))
        {
            _logger.LogInformation("Email omitido por preferencias del usuario para {Email}, tipo: {Tipo}", email, tipoNotificacion);
            return false;
        }

        var enviado = await _emailService.SendEmailAsync(emailNotification, cancellationToken);

        // Guardar en historial siempre (independiente de si se envió o no)
        await GuardarNotificacionAsync(
            email,
            tipoNotificacion,
            emailNotification.Subject,
            emailNotification.Body,
            "Email",
            "normal",
            cancellationToken: cancellationToken
        );

        return enviado;
    }

    /// <summary>
    /// Envía una notificación push verificando preferencias
    /// </summary>
    protected async Task<bool> EnviarPushAsync(
        string email,
        string tipoNotificacion,
        PushNotification pushNotification,
        CancellationToken cancellationToken = default)
    {
        if (!await DeboEnviarNotificacionAsync(email, "Push", tipoNotificacion, cancellationToken))
        {
            _logger.LogInformation("Push omitido por preferencias del usuario para {Email}, tipo: {Tipo}", email, tipoNotificacion);
            return false;
        }

        await _pushService.SendAsync(pushNotification, cancellationToken);

        // Guardar en historial siempre
        await GuardarNotificacionAsync(
            email,
            tipoNotificacion,
            pushNotification.Title,
            pushNotification.Message,
            "Push",
            pushNotification.Priority.ToString().ToLower(),
            cancellationToken: cancellationToken
        );

        return true;
    }

    /// <summary>
    /// Envía notificación por ambos canales (email y push) verificando preferencias
    /// </summary>
    protected async Task EnviarNotificacionCompletaAsync(
        string email,
        string tipoNotificacion,
        EmailNotification emailNotification,
        PushNotification pushNotification,
        CancellationToken cancellationToken = default)
    {
        // Enviar email
        var emailEnviado = await EnviarEmailAsync(email, tipoNotificacion, emailNotification, cancellationToken);

        if (emailEnviado)
        {
            _logger.LogInformation("Email enviado a {Email} para tipo {Tipo}", email, tipoNotificacion);
        }
        else
        {
            _logger.LogWarning("Fallo al enviar email a {Email} para tipo {Tipo}", email, tipoNotificacion);
        }

        // Enviar push
        var pushEnviado = await EnviarPushAsync(email, tipoNotificacion, pushNotification, cancellationToken);

        if (pushEnviado)
        {
            _logger.LogInformation("Push enviado a {Email} para tipo {Tipo}", email, tipoNotificacion);
        }
    }
}
