namespace Reservas.Application.DTOs;

public class ReservaActividadDto
{
    public int ReservaActividadId { get; set; }
    public int ActividadId { get; set; }
    public int HuespedId { get; set; }
    public DateTime FechaReserva { get; set; }
    public TimeSpan HoraReserva { get; set; }
    public int NumeroPersonas { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public decimal MontoTotal { get; set; }
    public string? NotasEspeciales { get; set; }
    public bool RecordatorioEnviado { get; set; }
    public DateTime? FechaRecordatorio { get; set; }
    public ActividadRecreativaDto? Actividad { get; set; }
}
