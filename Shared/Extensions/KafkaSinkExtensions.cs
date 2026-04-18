using Serilog;
using Serilog.Configuration;
using Shared.Kafka.Settings;
using Shared.Kafka.Sinks;
using Shared.Kafka.Topics;

namespace Shared.Extensions;

public static class KafkaSinkExtensions
{
    /// <summary>
    /// Serilog LoggerConfiguration'a Kafka sink ekler.
    /// .WriteTo.KafkaSink(settings, "OrderService")
    /// </summary>
    public static LoggerConfiguration KafkaSink(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        KafkaSettings settings,
        string serviceName,
        string topic = KafkaTopics.Log.Entry)
    {
        return loggerSinkConfiguration.Sink(new KafkaSink(settings, serviceName, topic));
    }
}
