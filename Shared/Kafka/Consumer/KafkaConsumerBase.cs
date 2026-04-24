using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Kafka.Settings;
using System.Text.Json;

namespace Shared.Kafka.Consumer;

/// <summary>
/// Generic Kafka consumer base. Her servis bu sınıftan türetir, sadece HandleAsync'i implement eder.
/// </summary>
/// <example>
/// public class OrderCreatedConsumer : KafkaConsumerBase&lt;OrderCreatedEvent&gt;
/// {
///     public OrderCreatedConsumer(KafkaSettings settings, ILogger&lt;OrderCreatedConsumer&gt; logger)
///         : base(settings, KafkaTopics.Order.Created, logger) { }
///
///     protected override async Task HandleAsync(OrderCreatedEvent message, CancellationToken ct)
///     {
///         // business logic
///     }
/// }
/// </example>
public abstract class KafkaConsumerBase<TMessage> : BackgroundService where TMessage : class
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger _logger;
    private readonly string _topic;

    protected KafkaConsumerBase(KafkaSettings settings, string topic, ILogger logger)
    {
        _topic  = topic;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            GroupId          = settings.GroupId,
            AutoOffsetReset  = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            SessionTimeoutMs = settings.SessionTimeoutMs
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _consumer.Subscribe(_topic);
    }

    protected abstract Task HandleAsync(TMessage message, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Kafka consumer started. Topic: {Topic}", _topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);

                if (result?.Message?.Value is null)
                    continue;

                var message = JsonSerializer.Deserialize<TMessage>(result.Message.Value);
                if (message is null)
                    continue;

                await HandleAsync(message, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                    _consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error on topic {Topic}: {Reason}", _topic, ex.Error.Reason);
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in consumer for topic {Topic}", _topic);
                await Task.Delay(1000, stoppingToken);
            }
        }

        _consumer.Close();
        _logger.LogInformation("Kafka consumer stopped. Topic: {Topic}", _topic);
    }

    public override void Dispose()
    {
        _consumer.Dispose();
        base.Dispose();
    }
}
