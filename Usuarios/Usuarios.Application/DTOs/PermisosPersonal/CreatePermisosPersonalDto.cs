namespace Usuarios.Application.DTOs.PermisosPersonal;

public class CreatePermisosPersonalDto
{
    public int PersonalId { get; set; }
    public int? HabitacionId { get; set; }
    public string TipoPermiso { get; set; } = string.Empty;
    public DateTime? FechaExpiracion { get; set; }
    public bool EsTemporal { get; set; }
    public string OtorgadoPor { get; set; } = string.Empty;
    public string? Justificacion { get; set; }
}
