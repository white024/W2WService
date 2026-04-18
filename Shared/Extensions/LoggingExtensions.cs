using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Shared.Kafka.Settings;

namespace Shared.Extensions;

public static class LoggingExtensions
{
    /// <summary>
    /// Program.cs'de kullanım:
    /// builder.Host.AddSharedLogging("OrderService");
    ///
    /// Tüm log'lar → Console + SEQ + Kafka (Log Service → MongoDB)
    /// </summary>
    public static IHostBuilder AddSharedLogging(this IHostBuilder hostBuilder, string serviceName)
    {
        return hostBuilder.UseSerilog((ctx, config) =>
        {
            var seqUrl = ctx.Configuration["Seq:Url"] ?? "http://localhost:5341";
            var kafkaSettings = ctx.Configuration.GetSection("Kafka").Get<KafkaSettings>()
                                ?? new KafkaSettings();

            config
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ServiceName", serviceName)
                .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName)
                .Enrich.WithMachineName()
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj} {Properties}{NewLine}{Exception}")
                .WriteTo.Seq(seqUrl,
                    restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.KafkaSink(kafkaSettings, serviceName);
        });
    }
}