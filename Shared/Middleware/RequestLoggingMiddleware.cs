using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Extensions;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Shared.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    private static readonly string[] SensitiveFields = ["refreshToken", "password", "token"];

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();

            buffer.Position = 0;
            var responseBody = await new StreamReader(buffer).ReadToEndAsync();

            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            var safeBody = MaskSensitiveFields(responseBody);

            var level = context.Response.StatusCode >= 500 ? LogLevel.Error
                      : context.Response.StatusCode >= 400 ? LogLevel.Warning
                      : LogLevel.Information;

            _logger.Log(level,
                "HTTP {Method} {Path} → {StatusCode} in {DurationMs}ms | CorrelationId: {CorrelationId} | Response: {ResponseBody}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                context.GetCorrelationId(),
                safeBody);
        }
    }

    private static string MaskSensitiveFields(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return json;

        foreach (var field in SensitiveFields)
        {
            json = Regex.Replace(
                json,
                $@"""{field}""\s*:\s*""[^""]+""",
                $@"""{field}"": ""***""",
                RegexOptions.IgnoreCase);
        }

        return json;
    }
}