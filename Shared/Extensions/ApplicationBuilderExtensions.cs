using Microsoft.AspNetCore.Builder;
using Shared.Middleware;

namespace Shared.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Tüm servislerin Program.cs'inde çağırması gereken shared middleware'ler.
    /// app.UseSharedMiddleware();
    /// </summary>
    public static IApplicationBuilder UseSharedMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
}