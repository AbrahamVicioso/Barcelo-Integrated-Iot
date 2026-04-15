namespace Dispositivos.Domain.Entities;

public class EventosSistema
{
    public long EventoId { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public string Severidad { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Origen { get; set; } = string.Empty;
    public DateTime FechaOcurrencia { get; set; }
    public bool EstaResuelto { get; set; }
    public string? ResueltoPor { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public string? SolucionAplicada { get; set; }
    public Guid? DispositivoId { get; set; }
    public int? CerraduraId { get; set; }
    public int? HotelId { get; set; }
}
