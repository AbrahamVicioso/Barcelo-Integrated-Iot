namespace Notification.Domain.Data;

/// <summary>
/// Preferencias de notificación de un usuario
/// </summary>
public class PreferenciaNotificacion
{
    public int PreferenciaId { get; set; }
    public string UsuarioId { get; set; } = string.Empty;

    // Canales de notificación
    public bool HabilitarNotificacionesPush { get; set; } = true;
    public bool HabilitarNotificacionesEmail { get; set; } = true;
    public bool HabilitarNotificacionesSMS { get; set; } = false;

    // Tipos de notificaciones
    public bool NotificarAccesoPersonal { get; set; } = true;
    public bool NotificarRecordatorioActividad { get; set; } = true;
    public bool NotificarPromocionesOfertas { get; set; } = true;
    public bool NotificarReservas { get; set; } = true;
    public bool NotificarCredenciales { get; set; } = true;
    public bool NotificarCheckIn { get; set; } = true;
    public bool NotificarCuentaCreada { get; set; } = true;
    public bool NotificarConfirmacionEmail { get; set; } = true;
    public bool NotificarRestablecerPassword { get; set; } = true;

    // Horario no molestar
    public bool HorarioNoMolestar { get; set; } = false;
    public TimeSpan? HoraInicioNoMolestar { get; set; }
    public TimeSpan? HoraFinNoMolestar { get; set; }

    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}
