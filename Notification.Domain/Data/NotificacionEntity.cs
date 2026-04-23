namespace Notification.Domain.Data;

/// <summary>
/// Historial de notificaciones enviadas a usuarios
/// </summary>
public class NotificacionEntity
{
    public long NotificacionId { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string TipoNotificacion { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Prioridad { get; set; } = "normal";
    public bool FueLeida { get; set; } = false;
    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
    public DateTime? FechaLectura { get; set; }
    public string? TipoEntidadRelacionada { get; set; }
    public int? EntidadRelacionadaId { get; set; }
    public string CanalEnvio { get; set; } = string.Empty;
    public string EstadoEnvio { get; set; } = string.Empty;
}
