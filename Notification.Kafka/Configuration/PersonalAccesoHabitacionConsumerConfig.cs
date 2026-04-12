namespace Notification.Kafka.Configuration
{
    public class PersonalAccesoHabitacionConsumerConfig : KafkaConsumerConfig
    {
        public PersonalAccesoHabitacionConsumerConfig()
        {
            GroupId = "notification-personal-acceso-group";
            Topic = "habitacion.personal-acceso";
        }
    }
}
