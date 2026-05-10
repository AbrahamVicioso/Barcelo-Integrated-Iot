namespace Notification.Domain.Events;

public class ActividadFechaActualizadaEvent
{
    public int ReservaActividadId { get; set; }
    public int ActividadId { get; set; }
    public DateTime FechaReserva { get; set; }
    public TimeSpan HoraReserva { get; set; }
    public int? DuracionMinutos { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
