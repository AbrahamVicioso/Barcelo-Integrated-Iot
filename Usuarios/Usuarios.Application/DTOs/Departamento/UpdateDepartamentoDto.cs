namespace Usuarios.Application.DTOs.Departamento;

public class UpdateDepartamentoDto
{
    public int DepartamentoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EstaActivo { get; set; }
}
