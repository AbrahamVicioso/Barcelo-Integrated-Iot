namespace Usuarios.Application.DTOs.Personal;

public class UpdatePersonalDto
{
    public int PersonalId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public int PuestoId { get; set; }
    public int DepartamentoId { get; set; }
    public bool EstaActivo { get; set; }
    public string? Turno { get; set; }
    public int? Supervisor { get; set; }
}
