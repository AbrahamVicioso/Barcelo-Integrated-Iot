namespace Reservas.Application.DTOs;

public class ActividadRecreativaDto
{
    public int ActividadId { get; set; }
    public int HotelId { get; set; }
    public string NombreActividad { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public TimeSpan HoraApertura { get; set; }
    public TimeSpan HoraCierre { get; set; }
    public int CapacidadMaxima { get; set; }
    public decimal PrecioPorPersona { get; set; }
    public bool RequiereReserva { get; set; }
    public int? DuracionMinutos { get; set; }
    public bool EstaActiva { get; set; }
    public string? ImagenUrl { get; set; }
    public DateTime FechaCreacion { get; set; }
}
