namespace Notification.Domain.Events
{
    public class UnlockDoorEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int ReservaId { get; set; }
        public int HabitacionId { get; set; }
        public string NumeroReserva { get; set; } = string.Empty;
        public int? CredencialId { get; set; }
        public string? UsuarioId { get; set; }
        public string? DireccionIp { get; set; }
        public string? InfoDispositivo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
