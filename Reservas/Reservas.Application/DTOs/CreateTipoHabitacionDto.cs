namespace Reservas.Application.DTOs;

public class CreateTipoHabitacionDto
{
    public int TipoHabitacionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
