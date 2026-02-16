namespace Dispositivos.Application.DTOs;

public class RegistrosAuditoriumDto
{
    public long AuditoriaId { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string TipoEntidad { get; set; } = string.Empty;
    public int? EntidadId { get; set; }
    public string ValorAnterior { get; set; } = string.Empty;
    public string ValorNuevo { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public string DireccionIp { get; set; } = string.Empty;
    public string AgenteUsuario { get; set; } = string.Empty;
    public string Resultado { get; set; } = string.Empty;
    public string MensajeError { get; set; } = string.Empty;
    public int? HotelId { get; set; }
}
