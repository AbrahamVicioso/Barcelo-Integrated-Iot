namespace Notification.Domain.Events
{
    public class ActividadUnlockDoorEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int ReservaActividadId { get; set; }
        public int ActividadId { get; set; }
        public string NombreActividad { get; set; } = string.Empty;
        public int? CredencialId { get; set; }
        public string? UsuarioId { get; set; }
        public string? DireccionIp { get; set; }
        public string? InfoDispositivo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
