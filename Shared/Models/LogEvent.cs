using Shared.Enums.Common;

namespace Shared.Models;

/// <summary>
/// MongoDB'deki logs koleksiyonuna yazılan döküman modeli.
/// LogService, Kafka'dan gelen LogEntryEvent'i bu modele map'leyip kaydeder.
/// </summary>
public class LogEvent
{
    public string ServiceName { get; set; } = string.Empty;
    public string? Path { get; set; }
    public string? Method { get; set; }
    public int StatusCode { get; set; }
    public long DurationMs { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public LogCategory? Category { get; init; }
    public string? Exception { get; init; }
    public Dictionary<string, string> Properties { get; init; } = [];
}
