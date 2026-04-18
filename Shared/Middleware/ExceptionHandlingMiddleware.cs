using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Models;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception on {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteErrorResponse(context, ex);
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            ArgumentNullException => (HttpStatusCode.BadRequest, ex.Message),
            ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            InvalidOperationException => (HttpStatusCode.UnprocessableEntity, ex.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        context.Response.StatusCode  = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = ReturnObject<object>.Fail(message);
        response.ResultMessage = context.TraceIdentifier;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}