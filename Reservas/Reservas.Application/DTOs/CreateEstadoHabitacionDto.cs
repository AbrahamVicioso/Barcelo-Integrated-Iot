namespace Reservas.Application.DTOs;

public class CreateEstadoHabitacionDto
{
    public int EstadoHabitacionId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}
