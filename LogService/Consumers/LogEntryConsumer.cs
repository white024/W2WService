using MongoDB.Driver;
using Shared.Contracts.Events.Log;
using Shared.Kafka.Consumer;
using Shared.Kafka.Settings;
using Shared.Kafka.Topics;
using Shared.Models;

namespace LogService.Consumers;

/// <summary>
/// log.entry topic'ini consume eder, MongoDB'ye yazar.
/// Program.cs'de: builder.Services.AddKafkaConsumer&lt;LogEntryConsumer&gt;();
/// </summary>
public class LogEntryConsumer : KafkaConsumerBase<LogEntryEvent>
{
    private readonly IMongoCollection<LogEvent> _collection;

    public LogEntryConsumer(
        KafkaSettings settings,
        IMongoDatabase db,
        ILogger<LogEntryConsumer> logger)
        : base(settings, KafkaTopics.Log.Entry, logger)
    {
        _collection = db.GetCollection<LogEvent>("logs");
    }

    protected override async Task HandleAsync(LogEntryEvent message, CancellationToken ct)
    {
        var logEvent = new LogEvent
        {
            ServiceName   = message.ServiceName,
            Level         = message.Level,
            Message       = message.Message,
            Path          = message.Path,
            Method        = message.Method,
            StatusCode    = message.StatusCode ?? 0,
            DurationMs    = message.DurationMs ?? 0,
            CorrelationId = message.CorrelationId,
            Timestamp     = message.OccurredAt,
            Category      = message.Category,
            Exception     = message.Exception,
            Properties    = message.Properties
        };

        await _collection.InsertOneAsync(logEvent, cancellationToken: ct);
    }
}
