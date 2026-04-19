using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var kafka = builder.AddKafka("kafka");
var seq = builder.AddSeq("seq");

var authService = builder.AddProject<AuthService>("auth")
    .WithReference(seq)
    .WithReference(kafka)
     .WithExternalHttpEndpoints();

var catalogService = builder.AddProject<CatalogService>("catalog")
  .WithReference(seq)
    .WithReference(kafka)
     .WithExternalHttpEndpoints();


var cartService = builder.AddProject<CartService>("cart")
  .WithReference(seq)
    .WithReference(kafka)
     .WithExternalHttpEndpoints();


var orderService = builder.AddProject<OrderManagementService>("order")
    .WithReference(kafka)
    .WithReference(seq)
     .WithExternalHttpEndpoints();


var paymentService = builder.AddProject<PaymentService>("payment")
    .WithReference(kafka)
    .WithReference(seq)
     .WithExternalHttpEndpoints();


var historyService = builder.AddProject<HistoryService>("history")
    .WithReference(kafka)
    .WithReference(seq)
     .WithExternalHttpEndpoints();


var logService = builder.AddProject<LogService>("log")
    .WithReference(kafka)
    .WithReference(seq)
     .WithExternalHttpEndpoints();


var gateway = builder.AddProject<GatewayService>("gateway")
    .WithReference(authService.GetEndpoint("http"))
    .WithReference(catalogService.GetEndpoint("http"))
    .WithReference(cartService.GetEndpoint("http"))
    .WithReference(orderService.GetEndpoint("http"))
    .WithReference(paymentService.GetEndpoint("http"))
    .WithReference(historyService.GetEndpoint("http"))
    .WithReference(seq.GetEndpoint("http"))
     .WithExternalHttpEndpoints();

await builder.Build().RunAsync();