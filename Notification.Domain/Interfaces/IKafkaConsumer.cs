namespace Notification.Domain.Interfaces
{
    public interface IKafkaConsumer : IDisposable
    {
        Task StartAsync(CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);
        bool IsRunning { get; }
    }
}
