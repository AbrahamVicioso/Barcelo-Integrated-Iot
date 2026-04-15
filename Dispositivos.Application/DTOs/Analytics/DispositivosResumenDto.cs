namespace Dispositivos.Application.DTOs.Analytics;

public class DispositivosResumenDto
{
    public int Total { get; set; }
    public int Operativos { get; set; }
    public int Mantenimiento { get; set; }
    public int Falla { get; set; }
    public int Inactivos { get; set; }
    public decimal SaludGeneral { get; set; }
    public int BateriaBaja { get; set; }
    public int Offline { get; set; }
    public int FirmwareDesactualizado { get; set; }
}
