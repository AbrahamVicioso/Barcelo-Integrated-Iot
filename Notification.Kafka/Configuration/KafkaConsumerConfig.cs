namespace Notification.Kafka.Configuration
{
    public class KafkaConsumerConfig
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string AutoOffsetReset { get; set; } = "Earliest";
        public bool EnableAutoCommit { get; set; } = true;
        public int AutoCommitIntervalMs { get; set; } = 5000;
        public int SessionTimeoutMs { get; set; } = 30000;
        public int MaxPollIntervalMs { get; set; } = 300000;
    }
}
