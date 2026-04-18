using Shared.Enums;

namespace Shared.Contracts.Events.Order;

public class OrderCreatedEvent : BaseEvent
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public Guid CompanyId { get; init; }
    public Guid CartId { get; init; }
    public decimal TotalAmount { get; init; }
    public List<OrderItemDto> Items { get; init; } = [];
}

public class OrderStatusChangedEvent : BaseEvent
{
    public Guid OrderId { get; init; }
    public OrderStatus OldStatus { get; init; }
    public OrderStatus NewStatus { get; init; }
    public string? Reason { get; init; }
}

public class OrderCancelledEvent : BaseEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record OrderItemDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);