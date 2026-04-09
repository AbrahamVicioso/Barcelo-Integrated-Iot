namespace Notification.Kafka.Configuration
{
    public class CredencialesCheckInConsumerConfig : KafkaConsumerConfig
    {
        public CredencialesCheckInConsumerConfig()
        {
            GroupId = "notification-credenciales-checkin-group";
            Topic = "checkin.credenciales";
        }
    }
}
