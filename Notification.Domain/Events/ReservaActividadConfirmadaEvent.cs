namespace Notification.Domain.Events;

public class ReservaActividadConfirmadaEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int ReservaActividadId { get; set; }
    public int ActividadId { get; set; }
    public int HuespedId { get; set; }
    public string? Email { get; set; }
    public string? NombreCompleto { get; set; }
    public DateTime FechaReserva { get; set; }
    public TimeSpan HoraReserva { get; set; }
    public int? DuracionMinutos { get; set; }
    public string NombreActividad { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
