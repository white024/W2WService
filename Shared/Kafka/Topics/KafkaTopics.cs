namespace Shared.Kafka.Topics;

public static class KafkaTopics
{
    public static class OrderTopics
    {
        public const string Created = "order.created";
        public const string Updated = "order.updated";
        public const string Cancelled = "order.cancelled";
        public const string StatusChanged = "order.status.changed";
    }

    public static class PaymentTopics
    {
        public const string Completed = "payment.completed";
        public const string Failed = "payment.failed";
        public const string Refunded = "payment.refunded";
    }

    public static class CatalogTopics
    {
        public const string ProductCreated = "catalog.product.created";
        public const string ProductUpdated = "catalog.product.updated";
        public const string PriceChanged = "catalog.price.changed";
    }

    public static class CartTopics
    {
        public const string CheckedOut = "cart.checked.out";
        public const string Cleared = "cart.cleared";
    }

    public static class UserTopics
    {
        public const string Registered = "user.registered";
        public const string Updated = "user.updated";
    }

    public static class LogTopics
    {
        public const string Entry = "log.entry";
    }
}