using AuthService.Indexes;
using AuthService.Mappings;
using AuthService.Repositories;
using AuthService.Services;
using AuthService.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Shared.Extensions;
using Shared.Filters;
using Shared.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSharedLogging("AuthService");
builder.Services.AddSharedJwtAuthentication(builder.Configuration);
builder.Services.AddSingleton<IMongoClient>(
    new MongoClient(builder.Configuration["MongoDB:ConnectionString"]));

builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>()
      .GetDatabase(builder.Configuration["MongoDB:DatabaseName"]));

builder.Services.AddSharedKafka(builder.Configuration);

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IInviteRepository, InviteRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();

builder.Services.AddScoped<ApiKeyAuthFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiKeyAuthFilter>();
});

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<UserMappingProfile>();
    cfg.AddProfile<CompanyMappingProfile>();
    cfg.AddProfile<InviteMappingProfile>();
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginValidator>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(x => x.Errors)
            .Select(x => x.ErrorMessage)
            .ToList();

        var response = ReturnObject<object>.Fail(string.Join(", ", errors));
        return new BadRequestObjectResult(response);
    };
});

builder.Services.AddSharedTokenService();
builder.Services.AddIndexInitializers(typeof(UserIndexInitializer).Assembly);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseSharedMiddleware();
app.UseAuthentication();
app.UseAuthorization(); 
app.MapControllers();

await app.RunAsync();