namespace Usuarios.Application.DTOs.Departamento;

public class DepartamentoDto
{
    public int DepartamentoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EstaActivo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? EliminadoEn { get; set; }
}
