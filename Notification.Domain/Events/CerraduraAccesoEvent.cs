namespace Notification.Domain.Events
{
    public class CerraduraAccesoEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string DeviceName { get; set; } = string.Empty;
        public long Timestamp { get; set; }
        public CerraduraAccesoData Data { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class CerraduraAccesoData
    {
        public bool AccessGranted { get; set; }
        public string AccessMethod { get; set; } = string.Empty;
        public string? CredTipo { get; set; }
        public int? CredId { get; set; }
        public int? ReservaId { get; set; }
        public long? CredPin { get; set; }
        public DateTime? CredAct { get; set; }
        public DateTime? CredExp { get; set; }
    }
}
