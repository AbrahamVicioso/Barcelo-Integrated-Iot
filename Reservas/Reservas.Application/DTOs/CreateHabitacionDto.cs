namespace Reservas.Application.DTOs;

public class CreateHabitacionDto
{
    public int HotelId { get; set; }
    public string NumeroHabitacion { get; set; } = string.Empty;
    public string TipoHabitacion { get; set; } = string.Empty;
    public int Piso { get; set; }
    public int CapacidadMaxima { get; set; } = 2;
    public decimal PrecioPorNoche { get; set; }
    public string Estado { get; set; } = "Disponible";
    public bool EstaDisponible { get; set; } = true;
    public string? Descripcion { get; set; }
}
