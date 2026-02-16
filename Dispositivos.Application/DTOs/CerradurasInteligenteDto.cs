namespace Dispositivos.Application.DTOs;

public class CerradurasInteligenteDto
{
    public int CerraduraId { get; set; }
    public Guid DispositivoId { get; set; }
    public string? NombreDispositivo { get; set; }
    public int HabitacionId { get; set; }
    public string? NombreHabitacion { get; set; }
    public string EstadoPuerta { get; set; } = string.Empty;
    public DateTime? UltimaApertura { get; set; }
    public int ContadorAperturas { get; set; }
    public bool SoportaModoOffline { get; set; }
    public DateTime FechaActivacion { get; set; }
    public bool EstaActiva { get; set; }
    public DateTime FechaCreacion { get; set; }
}
