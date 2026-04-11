namespace Usuarios.Application.DTOs.PermisosPersonal;

public class CreatePermisosPersonalDto
{
    public int PersonalId { get; set; }
    public int? HabitacionId { get; set; }
    public int? ActividadId { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public bool EsTemporal { get; set; }
    public string? Justificacion { get; set; }
}
