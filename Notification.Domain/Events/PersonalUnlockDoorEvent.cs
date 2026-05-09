namespace Notification.Domain.Events;

public class PersonalUnlockDoorEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int HabitacionId { get; set; }
    public string NumeroHabitacion { get; set; } = string.Empty;
    public int PersonalId { get; set; }
    public string NombrePersonal { get; set; } = string.Empty;
    public string? UsuarioId { get; set; }
    public string? DireccionIp { get; set; }
    public string? InfoDispositivo { get; set; }
    public List<HuespedCheckInInfo> Huespedes { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
