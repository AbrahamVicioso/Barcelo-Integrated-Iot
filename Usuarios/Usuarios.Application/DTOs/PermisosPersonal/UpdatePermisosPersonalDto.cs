namespace Usuarios.Application.DTOs.PermisosPersonal;

public class UpdatePermisosPersonalDto
{
    public int PermisoId { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public bool EstaActivo { get; set; }
    public string? Justificacion { get; set; }
}
