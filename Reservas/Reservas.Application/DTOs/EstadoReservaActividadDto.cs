namespace Reservas.Application.DTOs;

public class EstadoReservaActividadDto
{
    public int EstadoReservaActividadId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}