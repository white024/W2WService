using AuthService.Data.Repositories;
using AuthService.Data.Repositories.Interfaces;
using AuthService.Data.Services;
using AuthService.Data.Services.Interfaces;
using AuthService.Mappings;
using AuthService.Validators;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Shared.Extensions;
using Shared.Filters;
using Shared.Grpc;
using Shared.Models;
using System.IdentityModel.Tokens.Jwt;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiKeyAuthFilter>();
});
builder.Services.AddOpenApi();

builder.Host.AddSharedLogging("AuthService");
builder.Services.AddSharedConfiguration(builder.Configuration);
builder.Services.AddSharedJwtAuthentication(builder.Configuration);
builder.Services.AddSharedTokenService();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddSingleton<IMongoClient>(
    new MongoClient(builder.Configuration["MongoDB:ConnectionString"]));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>()
      .GetDatabase(builder.Configuration["MongoDB:DatabaseName"]));

builder.Services.AddSharedKafka(builder.Configuration);

var grpcUrl = builder.Configuration["services:customer:grpc:0"]
    ?? throw new InvalidOperationException("CustomerService gRPC address missing");

builder.Services.AddGrpcClient<CustomerGrpc.CustomerGrpcClient>(o =>
{
    o.Address = new Uri(grpcUrl);
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ApiKeyAuthFilter>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(x => x.Errors)
            .Select(x => x.ErrorMessage)
            .ToList();
        return new BadRequestObjectResult(
            ReturnObject<object>.Fail(string.Join(", ", errors)));
    };
});
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<UserMappingProfile>();
    cfg.AddProfile<InviteMappingProfile>();
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseSharedMiddleware();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();