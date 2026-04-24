using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Extensions;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddSharedConfiguration(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JwtSettings>(config.GetSection("Jwt"));
        return services;
    }
}
