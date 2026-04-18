namespace Notification.Kafka.Configuration
{
    public class PasswordResetConsumerConfig : KafkaConsumerConfig
    {
        public PasswordResetConsumerConfig()
        {
            GroupId = "notification-password-reset-group";
            Topic = "password-reset";
        }
    }
}
