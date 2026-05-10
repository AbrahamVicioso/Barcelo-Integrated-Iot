namespace Notification.Domain.Events;

public class ReservaHuespedActualizadoEvent
{
    public int ReservaId { get; set; }
    public string NumeroReserva { get; set; } = string.Empty;
    public DateTime FechaCheckIn { get; set; }
    public DateTime FechaCheckOut { get; set; }
    /// <summary>HuespedIds con PuedeDesbloquearCerradura=true (incluye titular)</summary>
    public List<int> HuespedesAutorizados { get; set; } = new();
    /// <summary>Todos los HuespedIds actuales de la reserva (titular + adicionales)</summary>
    public List<int> TodosHuespedes { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
