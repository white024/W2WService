namespace Shared.Contracts.Events.Catalog;

public class ProductCreatedEvent : BaseEvent
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public Guid CategoryId { get; init; }
    public Guid CompanyId { get; init; }
}

public class ProductUpdatedEvent : BaseEvent
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid CompanyId { get; init; }
}

public class PriceChangedEvent : BaseEvent
{
    public Guid ProductId { get; init; }
    public decimal OldPrice { get; init; }
    public decimal NewPrice { get; init; }
    public Guid CompanyId { get; init; }
}   