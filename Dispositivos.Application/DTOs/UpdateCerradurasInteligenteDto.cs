namespace Dispositivos.Application.DTOs;

public class UpdateCerradurasInteligenteDto
{
    public int CerraduraId { get; set; }
    public Guid DispositivoId { get; set; }
    public int HabitacionId { get; set; }
    public string EstadoPuerta { get; set; } = string.Empty;
    public DateTime? UltimaApertura { get; set; }
    public int ContadorAperturas { get; set; }
    public bool SoportaModoOffline { get; set; }
    public DateTime FechaActivacion { get; set; }
    public bool EstaActiva { get; set; }
}
