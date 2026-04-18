using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Kafka.Interfaces;
using Shared.Kafka.Producer;
using Shared.Kafka.Settings;

namespace Shared.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Kafka producer'ı DI'a ekler.
    /// builder.Services.AddSharedKafka(builder.Configuration);
    /// </summary>
    public static IServiceCollection AddSharedKafka(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("Kafka").Get<KafkaSettings>() ?? new KafkaSettings();
        services.AddSingleton(settings);
        services.AddSingleton<IKafkaProducer, KafkaProducer>();
        return services;
    }

    /// <summary>
    /// Kafka consumer'ı BackgroundService olarak ekler.
    /// builder.Services.AddKafkaConsumer&lt;OrderCreatedConsumer&gt;();
    /// </summary>
    public static IServiceCollection AddKafkaConsumer<TConsumer>(this IServiceCollection services)
        where TConsumer : BackgroundService
    {
        services.AddHostedService<TConsumer>();
        return services;
    }
}