// Shared/Extensions/TokenExtensions.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Shared.Extensions;

public static class TokenExtensions
{
    public static IServiceCollection AddSharedTokenService(
        this IServiceCollection services)
    {
        services.AddSingleton<TokenService>();
        return services;
    }
    public static IServiceCollection AddSharedJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                    ValidateIssuer   = true,
                    ValidIssuer      = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience    = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew        = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var token = ctx.Request.Cookies["accessToken"];
                        if (!string.IsNullOrEmpty(token))
                            ctx.Token = token;
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}