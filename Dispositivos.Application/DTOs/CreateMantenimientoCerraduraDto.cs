namespace Dispositivos.Application.DTOs;

public class CreateMantenimientoCerraduraDto
{
    public Guid? DispositivoId { get; set; }
    public int? CerraduraId { get; set; }
    public string TipoMantenimiento { get; set; } = string.Empty;
    public DateTime FechaProgramada { get; set; }
    public int? PersonalId { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    public decimal? CostoMantenimiento { get; set; }
    public int? TiempoEmpleadoMinutos { get; set; }
}
