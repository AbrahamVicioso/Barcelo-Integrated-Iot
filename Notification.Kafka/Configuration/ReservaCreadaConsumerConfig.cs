namespace Notification.Kafka.Configuration
{
    public class ReservaCreadaConsumerConfig
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string GroupId { get; set; } = "notification-reserva-created-group";
        public string Topic { get; set; } = "reserva-created";
        public string AutoOffsetReset { get; set; } = "Earliest";
        public bool EnableAutoCommit { get; set; } = true;
        public int AutoCommitIntervalMs { get; set; } = 5000;
        public int SessionTimeoutMs { get; set; } = 30000;
        public int MaxPollIntervalMs { get; set; } = 300000;
    }
}
