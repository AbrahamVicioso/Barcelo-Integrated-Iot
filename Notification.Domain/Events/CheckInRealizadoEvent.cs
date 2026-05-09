namespace Notification.Domain.Events;

public class CheckInRealizadoEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int ReservaId { get; set; }
    public string NumeroReserva { get; set; } = string.Empty;
    public List<HuespedCheckInInfo> Huespedes { get; set; } = new();
    public DateTime FechaCheckIn { get; set; }
    public DateTime FechaCheckOut { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class HuespedCheckInInfo
{
    public int HuespedId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
}
