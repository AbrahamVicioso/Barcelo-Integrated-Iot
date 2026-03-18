namespace Notification.Domain.Events
{
    public class UnlockDoorEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int ReservaId { get; set; }
        public int HabitacionId { get; set; }
        public string NumeroReserva { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
