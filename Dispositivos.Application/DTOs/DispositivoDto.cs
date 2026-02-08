namespace Dispositivos.Application.DTOs;

public class DispositivoDto
{
    public int DispositivoId { get; set; }
    public int HotelId { get; set; }
    public string? NombreHotel { get; set; }
    public string NumeroSerieDispositivo { get; set; } = string.Empty;
    public string DireccionMac { get; set; } = string.Empty;
    public string TipoDispositivo { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string VersionFirmware { get; set; } = string.Empty;
    public int NivelBateria { get; set; }
    public bool EstaEnLinea { get; set; }
    public DateTime? UltimaSincronizacion { get; set; }
    public DateTime FechaInstalacion { get; set; }
    public string EstadoFuncional { get; set; } = string.Empty;
    public DateTime? UltimaActualizacionFirmware { get; set; }
    public string Ipdispositivo { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}
