namespace Reservas.Application.DTOs;

public class CredencialHuespedDto
{
    public int CredencialId { get; set; }
    public string CodigoPIN { get; set; }
    public DateTime FechaActivacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public string TipoCredencial { get; set; }
    public bool EstaActiva { get; set; }
}
