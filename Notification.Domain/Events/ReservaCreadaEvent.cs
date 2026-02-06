namespace Notification.Domain.Events
{
    public class ReservaCreadaEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string NumeroReserva { get; set; } = string.Empty;
        public DateTime FechaCheckIn { get; set; }
        public DateTime FechaCheckOut { get; set; }
        public decimal MontoTotal { get; set; }
        public string HabitacionNumero { get; set; } = string.Empty;
        public string HotelNombre { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
