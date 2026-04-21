namespace Notification.Domain.Events;

public class PersonalAccesoHabitacionEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int HabitacionId { get; set; }
    public string NumeroHabitacion { get; set; } = string.Empty;
    public int PersonalId { get; set; }
    public string NombrePersonal { get; set; } = string.Empty;
    public string? Puesto { get; set; }
    public string? Departamento { get; set; }
    public bool EsAccesoFisico { get; set; }
    public List<HuespedCheckInInfo> Huespedes { get; set; } = new();
    public DateTime FechaAcceso { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
