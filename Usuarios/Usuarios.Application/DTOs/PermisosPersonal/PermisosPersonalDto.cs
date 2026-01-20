namespace Usuarios.Application.DTOs.PermisosPersonal;

public class PermisosPersonalDto
{
    public int PermisoId { get; set; }
    public int PersonalId { get; set; }
    public int? HabitacionId { get; set; }
    public string TipoPermiso { get; set; } = string.Empty;
    public DateTime FechaOtorgamiento { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public bool EsTemporal { get; set; }
    public string OtorgadoPor { get; set; } = string.Empty;
    public bool EstaActivo { get; set; }
    public string? Justificacion { get; set; }
}
