namespace Usuarios.Application.DTOs.Puesto;

public class UpdatePuestoDto
{
    public int PuestoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EstaActivo { get; set; }
}
