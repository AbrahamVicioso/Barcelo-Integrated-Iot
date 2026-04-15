namespace Reservas.Application.DTOs.Analytics;

public class ActividadPopularDto
{
    public int ActividadId { get; set; }
    public string Actividad { get; set; } = string.Empty;
    public int Reservas { get; set; }
}
