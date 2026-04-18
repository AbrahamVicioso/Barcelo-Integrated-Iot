using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Notification.Domain.Events;

namespace Authentication.Api.Services
{
    public interface IKafkaProducerService
    {
        Task PublishUserCreatedAsync(UserCreatedEvent userEvent, CancellationToken cancellationToken = default);
        Task PublishEmailConfirmationAsync(EmailConfirmationEvent confirmationEvent, CancellationToken cancellationToken = default);
        Task PublishPasswordResetAsync(PasswordResetEvent resetEvent, CancellationToken cancellationToken = default);
    }

    public class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;
        private readonly string _emailConfirmationTopic;
        private readonly string _passwordResetTopic;
        private readonly ILogger<KafkaProducerService> _logger;
        private bool _disposed;

        public KafkaProducerService(KafkaProducerConfig config, ILogger<KafkaProducerService> logger)
        {
            _topic = config.Topic;
            _emailConfirmationTopic = config.EmailConfirmationTopic;
            _passwordResetTopic = config.PasswordResetTopic;
            _logger = logger;

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config.BootstrapServers,
                ClientId = config.ClientId ?? "authentication-api",
                Acks = Acks.Leader,
                EnableDeliveryReports = true
            };

            _producer = new ProducerBuilder<string, string>(producerConfig)
                .SetErrorHandler((_, e) => _logger.LogError("Kafka producer error: {Reason}", e.Reason))
                .Build();
        }

        public async Task PublishUserCreatedAsync(UserCreatedEvent userEvent, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new Message<string, string>
                {
                    Key = userEvent.Email,
                    Value = JsonSerializer.Serialize(userEvent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })
                };

                var result = await _producer.ProduceAsync(_topic, message, cancellationToken);

                _logger.LogInformation("Published UserCreatedEvent for {Email} to partition {Partition} at offset {Offset}",
                    userEvent.Email, result.Partition.Value, result.Offset.Value);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to publish UserCreatedEvent for {Email}", userEvent.Email);
                throw;
            }
        }

        public async Task PublishEmailConfirmationAsync(EmailConfirmationEvent confirmationEvent, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new Message<string, string>
                {
                    Key = confirmationEvent.Email,
                    Value = JsonSerializer.Serialize(confirmationEvent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })
                };

                var result = await _producer.ProduceAsync(_emailConfirmationTopic, message, cancellationToken);

                _logger.LogInformation("Published EmailConfirmationEvent for {Email} to partition {Partition} at offset {Offset}",
                    confirmationEvent.Email, result.Partition.Value, result.Offset.Value);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to publish EmailConfirmationEvent for {Email}", confirmationEvent.Email);
                throw;
            }
        }

        public async Task PublishPasswordResetAsync(PasswordResetEvent resetEvent, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new Message<string, string>
                {
                    Key = resetEvent.Email,
                    Value = JsonSerializer.Serialize(resetEvent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })
                };

                var result = await _producer.ProduceAsync(_passwordResetTopic, message, cancellationToken);

                _logger.LogInformation("Published PasswordResetEvent for {Email} to partition {Partition} at offset {Offset}",
                    resetEvent.Email, result.Partition.Value, result.Offset.Value);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to publish PasswordResetEvent for {Email}", resetEvent.Email);
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
            _disposed = true;
        }
    }

    public class KafkaProducerConfig
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string Topic { get; set; } = "notifications";
        public string EmailConfirmationTopic { get; set; } = "email-confirmation";
        public string PasswordResetTopic { get; set; } = "password-reset";
        public string? ClientId { get; set; }
    }
}
