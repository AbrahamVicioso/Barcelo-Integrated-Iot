namespace Notification.Kafka.Configuration;

public class CredencialCreadaConsumerConfig : KafkaConsumerConfig
{
    public CredencialCreadaConsumerConfig()
    {
        GroupId = "notification-credencial-creada-group";
        Topic = "credenciales.creada";
    }
}