using CustomerService.Kafka.Consumers;
using Serilog;
using Shared.Extensions;
using Shared.Kafka.Interfaces;
using Shared.Kafka.Producer;
using Shared.Kafka.Settings;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Host.AddSharedLogging("CustomerService");

var kafkaSettings = builder.Configuration
    .GetSection("Kafka").Get<KafkaSettings>()!;

builder.Services.AddSingleton<IKafkaProducer>(sp =>
    new KafkaProducer(
        kafkaSettings,
        sp.GetRequiredService<ILogger<KafkaProducer>>()));

builder.Services.AddHostedService<UserRegisteredConsumer>();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration["Seq:Url"]!)
    .WriteTo.KafkaSink(kafkaSettings, "CustomerService")
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddSharedKafka(builder.Configuration);
builder.Services.AddSingleton(kafkaSettings);
builder.Services.AddHostedService<UserRegisteredConsumer>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
