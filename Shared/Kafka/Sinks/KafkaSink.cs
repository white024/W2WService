using Confluent.Kafka;
using Serilog.Core;
using Serilog.Events;
using Shared.Contracts.Events.Log;
using Shared.Enums.Common;
using Shared.Kafka.Settings;
using Shared.Kafka.Topics;
using System.Text.Json;

namespace Shared.Kafka.Sinks;

public sealed class KafkaSink : ILogEventSink, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _serviceName;
    private readonly string _topic;

    public KafkaSink(KafkaSettings settings, string serviceName, string topic = KafkaTopics.LogTopics.Entry)
    {
        _serviceName = serviceName;
        _topic       = topic;

        var config = new ProducerConfig
        {
            BootstrapServers     = settings.BootstrapServers,
            Acks                 = Acks.Leader,
            MessageSendMaxRetries = 1,
            SocketTimeoutMs      = 2000,
            MessageTimeoutMs     = 3000
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public void Emit(LogEvent logEvent)
    {
        try
        {
            var entry = new LogEntryEvent
            {
                ServiceName   = _serviceName,
                Level         = logEvent.Level.ToString(),
                Message       = logEvent.RenderMessage(),
                Exception     = logEvent.Exception?.ToString(),
                Category = Enum.TryParse<LogCategory>(GetProperty(logEvent, "Category"), out var cat) ? cat : null,
                CorrelationId = GetProperty(logEvent, "CorrelationId"),
                Properties    = logEvent.Properties
                    .Where(p => p.Key is not ("Category" or "CorrelationId" or "SourceContext"))
                    .ToDictionary(p => p.Key, p => p.Value.ToString().Trim('"'))
            };

            var payload = JsonSerializer.Serialize(entry);
            var message = new Message<string, string>
            {
                Key   = entry.EventId.ToString(),
                Value = payload
            };

            _producer.Produce(_topic, message);
        }
        catch
        {
        }
    }

    private static string? GetProperty(LogEvent logEvent, string key)
        => logEvent.Properties.TryGetValue(key, out var val)
            ? val.ToString().Trim('"')
            : null;

    public void Dispose() => _producer?.Dispose();
}