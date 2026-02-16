namespace Dispositivos.Application.DTOs;

public class UpdateCredencialesAccesoDto
{
    public int CredencialId { get; set; }
    public int? HuespedId { get; set; }
    public int? PersonalId { get; set; }
    public string CodigoPin { get; set; } = string.Empty;
    public DateTime FechaActivacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public bool EstaActiva { get; set; }
    public string TipoCredencial { get; set; } = string.Empty;
    public string CreadoPor { get; set; } = string.Empty;
    public int NumeroUsos { get; set; }
    public DateTime? UltimoUso { get; set; }
}
