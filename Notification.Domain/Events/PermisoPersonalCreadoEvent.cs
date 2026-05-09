namespace Notification.Domain.Events;

public class PermisoPersonalCreadoEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int? HabitacionId { get; set; }
    public int? ActividadId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
