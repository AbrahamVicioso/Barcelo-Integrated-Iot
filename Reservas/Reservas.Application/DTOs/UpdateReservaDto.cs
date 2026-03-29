namespace Reservas.Application.DTOs;

public class UpdateReservaDto
{
    public int ReservaId { get; set; }
    public int? HabitacionId { get; set; }
    public DateTime FechaCheckIn { get; set; }
    public DateTime FechaCheckOut { get; set; }
    public int NumeroHuespedes { get; set; }
    public int NumeroNinos { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal MontoPagado { get; set; }
    public int EstadoReservaId { get; set; }
    public string? Observaciones { get; set; }
}
