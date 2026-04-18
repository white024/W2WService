using MongoDB.Driver;
using Shared.Models;


namespace Shared.Kafka.Consumer;

public class LogConsumer
{
    private readonly IMongoCollection<LogEvent> _collection;

    public LogConsumer(IMongoDatabase db)
    {
        _collection = db.GetCollection<LogEvent>("logs");
    }

    public async Task Consume(LogEvent log)
    {
        await _collection.InsertOneAsync(log);
    }
}