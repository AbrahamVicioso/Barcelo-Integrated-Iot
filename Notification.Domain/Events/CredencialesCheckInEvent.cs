namespace Notification.Domain.Events;

public class CredencialesCheckInEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int ReservaId { get; set; }
    public string NumeroReserva { get; set; } = string.Empty;
    public DateTime FechaCheckIn { get; set; }
    public DateTime FechaCheckOut { get; set; }
    public List<CredencialHuespedInfo> Credenciales { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CredencialHuespedInfo
{
    public int HuespedId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string CodigoPin { get; set; } = string.Empty;
}
