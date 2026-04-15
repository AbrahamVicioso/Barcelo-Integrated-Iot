namespace Dispositivos.Application.DTOs.Analytics;

public class IncidentesResumenDto
{
    public int Activos { get; set; }
    public int Pendientes { get; set; }
    public int Resueltos { get; set; }
    public List<IncidentePorSeveridadDto> PorSeveridad { get; set; } = new();
}

public class IncidentePorSeveridadDto
{
    public string Severidad { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}
