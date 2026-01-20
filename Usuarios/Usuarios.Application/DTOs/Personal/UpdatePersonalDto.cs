namespace Usuarios.Application.DTOs.Personal;

public class UpdatePersonalDto
{
    public int PersonalId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public bool EstaActivo { get; set; }
    public string? Turno { get; set; }
    public int? Supervisor { get; set; }
}
