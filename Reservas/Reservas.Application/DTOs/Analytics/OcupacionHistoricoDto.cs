namespace Reservas.Application.DTOs.Analytics;

public class OcupacionHistoricoDto
{
    public string Periodo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public int Mes { get; set; }
    public decimal Valor { get; set; }
}
