namespace Dispositivos.Application.DTOs;

public class RegistrosAccesoDto
{
    public long RegistroId { get; set; }
    public int CerraduraId { get; set; }
    public int? CredencialId { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public DateTime FechaHoraAcceso { get; set; }
    public string TipoAcceso { get; set; } = string.Empty;
    public string ResultadoAcceso { get; set; } = string.Empty;
    public string MotivoAcceso { get; set; } = string.Empty;
    public string DireccionIp { get; set; } = string.Empty;
    public string InfoDispositivo { get; set; } = string.Empty;
    public bool FueExitoso { get; set; }
    public string CodigoError { get; set; } = string.Empty;
    public int? Latencia { get; set; }
}
