namespace Reservas.Application.DTOs;

public class HabitacionDto
{
    public int HabitacionId { get; set; }
    public int HotelId { get; set; }
    public string? NombreHotel { get; set; }
    public string NumeroHabitacion { get; set; } = string.Empty;
    public string TipoHabitacion { get; set; } = string.Empty;
    public int Piso { get; set; }
    public int CapacidadMaxima { get; set; }
    public decimal PrecioPorNoche { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool EstaDisponible { get; set; }
    public string? Descripcion { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}
