namespace Notification.Kafka.Configuration
{
    public class TwoFactorCodeConsumerConfig : KafkaConsumerConfig
    {
        public TwoFactorCodeConsumerConfig()
        {
            GroupId = "notification-two-factor-group";
            Topic = "two-factor-code";
        }
    }
}