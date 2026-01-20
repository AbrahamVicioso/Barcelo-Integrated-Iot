namespace Usuarios.Application.DTOs.Personal;

public class PersonalDto
{
    public int PersonalId { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public int HotelId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string NumeroEmpleado { get; set; } = string.Empty;
    public DateTime FechaContratacion { get; set; }
    public bool EstaActivo { get; set; }
    public string? Turno { get; set; }
    public int? Supervisor { get; set; }
    public DateTime FechaCreacion { get; set; }
}
