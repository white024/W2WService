using Confluent.Kafka;
using Serilog.Core;
using Serilog.Events;
using Shared.Contracts.Events.Log;
using Shared.Enums.Common;
using Shared.Kafka.Settings;
using Shared.Kafka.Topics;
using System.Text.Json;

namespace Shared.Kafka.Sinks;

/// <summary>
/// Serilog sink — her log event'i Kafka'ya publish eder.
/// Log Service bu topic'i consume edip MongoDB'ye yazar.
/// 
/// Kullanım (LoggingExtensions içinden otomatik eklenir):
/// .WriteTo.KafkaSink(kafkaSettings, serviceName)
/// </summary>
public sealed class KafkaSink : ILogEventSink, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _serviceName;
    private readonly string _topic;

    public KafkaSink(KafkaSettings settings, string serviceName, string topic = KafkaTopics.Log.Entry)
    {
        _serviceName = serviceName;
        _topic       = topic;

        var config = new ProducerConfig
        {
            BootstrapServers     = settings.BootstrapServers,
            Acks                 = Acks.Leader,
            MessageSendMaxRetries = 1,
            // Sink kendi hataları yutmalı, uygulamayı bloklamamalı
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

            // Fire-and-forget — sink asla uygulamayı bloklamamalı
            _producer.Produce(_topic, message);
        }
        catch
        {
            // Kafka'ya yazılamazsa sessizce geç — log kaybı, uygulama durmamalı
        }
    }

    private static string? GetProperty(LogEvent logEvent, string key)
        => logEvent.Properties.TryGetValue(key, out var val)
            ? val.ToString().Trim('"')
            : null;

    public void Dispose() => _producer?.Dispose();
}