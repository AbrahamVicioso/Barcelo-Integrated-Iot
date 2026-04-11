namespace Usuarios.Application.DTOs.Personal;

public class CreatePersonalDto
{
    public string UsuarioId { get; set; } = string.Empty;
    public int HotelId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public int PuestoId { get; set; }
    public int DepartamentoId { get; set; }
    public string NumeroEmpleado { get; set; } = string.Empty;
    public DateTime FechaContratacion { get; set; }
    public string? Turno { get; set; }
    public int? Supervisor { get; set; }
}
