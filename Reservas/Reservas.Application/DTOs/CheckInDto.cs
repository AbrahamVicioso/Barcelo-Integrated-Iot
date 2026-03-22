namespace Reservas.Application.DTOs;

public class CheckInDto
{
    public int CheckInId { get; set; }
    public int ReservaId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public DateTime FechaCheckIn { get; set; }
    public string Estado { get; set; } = string.Empty;
}
