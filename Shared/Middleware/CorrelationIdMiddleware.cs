using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Shared.Middleware;

public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

/// <summary>
/// Controller veya service'den correlation id almak için extension.
/// var id = HttpContext.GetCorrelationId();
/// </summary>
public static class HttpContextExtensions
{
    public static string? GetCorrelationId(this HttpContext context)
        => context.Items[CorrelationIdMiddleware.HeaderName]?.ToString();
}