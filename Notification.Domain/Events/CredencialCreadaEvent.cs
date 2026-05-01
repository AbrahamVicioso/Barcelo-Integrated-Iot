namespace Notification.Domain.Events;

public class CredencialCreadaEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int CredencialId { get; set; }
    public int? HuespedId { get; set; }
    public int? PersonalId { get; set; }
    public int? ReservaId { get; set; }
    public string? Email { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; } = string.Empty;
    public string CodigoPin { get; set; } = string.Empty;
    public DateTime FechaActivacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public string TipoCredencial { get; set; } = string.Empty;
    public string? NumeroReserva { get; set; }
    public int? ReservaActividadId { get; set; }
    public string? NombreActividad { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}