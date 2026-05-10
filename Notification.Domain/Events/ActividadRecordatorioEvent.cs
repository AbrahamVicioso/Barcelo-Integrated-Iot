namespace Notification.Domain.Events;

public class ActividadRecordatorioEvent
{
    public int ReservaActividadId { get; set; }
    public int ActividadId { get; set; }
    public string NombreActividad { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public DateTime FechaReserva { get; set; }
    public TimeSpan HoraReserva { get; set; }
    public int HuespedId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public int NumeroPersonas { get; set; }
    public int MinutosAntes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
