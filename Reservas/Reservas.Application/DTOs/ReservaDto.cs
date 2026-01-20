namespace Reservas.Application.DTOs;

public class ReservaDto
{
    public int ReservaId { get; set; }
    public int HuespedId { get; set; }
    public int HabitacionId { get; set; }
    public string NumeroReserva { get; set; } = string.Empty;
    public DateTime FechaCheckIn { get; set; }
    public DateTime FechaCheckOut { get; set; }
    public int NumeroHuespedes { get; set; }
    public int NumeroNinos { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal MontoPagado { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? CheckInRealizado { get; set; }
    public DateTime? CheckOutRealizado { get; set; }
    public string? CreadoPor { get; set; }
    public string? Observaciones { get; set; }
}
