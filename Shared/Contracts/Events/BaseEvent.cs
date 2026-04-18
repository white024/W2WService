namespace Shared.Contracts.Events;

public abstract class BaseEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
    public string? CorrelationId { get; init; }
}