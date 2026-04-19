// Shared/Extensions/IndexExtensions.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Indexes;
using System.Reflection;

namespace Shared.Extensions;

public static class IndexExtensions
{
    public static IServiceCollection AddIndexInitializers(
        this IServiceCollection services,
        Assembly assembly)
    {
        var initializerTypes = assembly.GetTypes()
            .Where(t => typeof(IIndexInitializer).IsAssignableFrom(t)
                     && !t.IsInterface
                     && !t.IsAbstract);

        foreach (var type in initializerTypes)
            services.AddScoped(typeof(IIndexInitializer), type);

        services.AddHostedService<IndexInitializerHostedService>();
        return services;
    }
}

public class IndexInitializerHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public IndexInitializerHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var initializers = scope.ServiceProvider.GetServices<IIndexInitializer>();
        foreach (var initializer in initializers)
            await initializer.InitializeAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}