namespace Notification.Domain.Events;

public class CheckInRealizadoEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int ReservaId { get; set; }
    public string NumeroReserva { get; set; } = string.Empty;
    public List<int> HuespedIds { get; set; } = new();
    public DateTime FechaCheckIn { get; set; }
    public DateTime FechaCheckOut { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
