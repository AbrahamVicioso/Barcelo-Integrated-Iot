namespace Usuarios.Application.DTOs.Puesto;

public class PuestoDto
{
    public int PuestoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EstaActivo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? EliminadoEn { get; set; }
}
