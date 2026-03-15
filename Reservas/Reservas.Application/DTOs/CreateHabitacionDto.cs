namespace Reservas.Application.DTOs;

public class CreateHabitacionDto
{
    public int HotelId { get; set; }
    public string NumeroHabitacion { get; set; } = string.Empty;
    public int TipoHabitacionId { get; set; } = 1;
    public int Piso { get; set; }
    public int CapacidadMaxima { get; set; } = 2;
    public decimal PrecioPorNoche { get; set; }
    public int EstadoHabitacionId { get; set; } = 1;
    public string? Descripcion { get; set; }
}
