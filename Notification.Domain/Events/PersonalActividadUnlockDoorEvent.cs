namespace Notification.Domain.Events
{
    public class PersonalActividadUnlockDoorEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int ActividadId { get; set; }
        public string NombreActividad { get; set; } = string.Empty;
        public int PersonalId { get; set; }
        public string NombrePersonal { get; set; } = string.Empty;
        public string? UsuarioId { get; set; }
        public string? DireccionIp { get; set; }
        public string? InfoDispositivo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
