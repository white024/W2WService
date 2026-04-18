using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Shared.Kafka.Interfaces;
using Shared.Kafka.Settings;
using System.Text.Json;

namespace Shared.Kafka.Producer;

public sealed class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(KafkaSettings settings, ILogger<KafkaProducer> logger)
    {
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers    = settings.BootstrapServers,
            Acks                = Acks.All,
            EnableDeliveryReports = true,
            MessageSendMaxRetries = settings.RetryCount,
            RetryBackoffMs      = settings.RetryBackoffMs,
            EnableIdempotence   = true
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public Task PublishAsync<T>(string topic, T message, CancellationToken cancellationToken = default)
        => PublishAsync(topic, Guid.NewGuid().ToString(), message, cancellationToken);

    public async Task PublishAsync<T>(string topic, string key, T message, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<string, string> { Key = key, Value = payload };

            var result = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);

            _logger.LogDebug(
                "Published to {Topic} [partition:{Partition} offset:{Offset}]",
                topic, result.Partition.Value, result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Kafka publish failed. Topic: {Topic}, Reason: {Reason}", topic, ex.Error.Reason);
            throw;
        }
    }

    public void Dispose() => _producer.Dispose();
}