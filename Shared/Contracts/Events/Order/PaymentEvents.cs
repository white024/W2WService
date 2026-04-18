using Shared.Enums;

namespace Shared.Contracts.Events.Payment;

public class PaymentCompletedEvent : BaseEvent
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string TransactionId { get; init; } = string.Empty;
    public PaymentMethod Method { get; init; }
}

public class PaymentFailedEvent : BaseEvent
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string FailureReason { get; init; } = string.Empty;
    public string? ErrorCode { get; init; }
}

public class PaymentRefundedEvent : BaseEvent
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public decimal RefundedAmount { get; init; }
    public string Reason { get; init; } = string.Empty;
}