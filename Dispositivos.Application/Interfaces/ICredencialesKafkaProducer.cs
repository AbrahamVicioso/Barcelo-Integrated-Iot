using Notification.Domain.Events;

namespace Dispositivos.Application.Interfaces;

public interface ICredencialesKafkaProducer
{
    Task PublishCredencialCreadaAsync(CredencialCreadaEvent credencialEvent, CancellationToken cancellationToken = default);
}

public class CredencialesKafkaProducerConfig
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string Topic { get; set; } = "credenciales.creada";
    public string ClientId { get; set; }
}