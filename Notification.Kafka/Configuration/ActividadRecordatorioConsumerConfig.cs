namespace Notification.Kafka.Configuration;

public class ActividadRecordatorioConsumerConfig : KafkaConsumerConfig
{
    public ActividadRecordatorioConsumerConfig()
    {
        GroupId = "notification-actividad-recordatorio-group";
        Topic = "actividades.recordatorio";
    }
}
