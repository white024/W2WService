using Microsoft.AspNetCore.Http;
using Shared.Services;

namespace GatewayService.Middleware;

public class RefreshTokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly TokenService _tokenService;

    public RefreshTokenMiddleware(
        RequestDelegate next,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        TokenService tokenService)
    {
        _next = next;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _tokenService = tokenService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsAuthEndpoint(context.Request.Path))
        {
            await _next(context);
            return;
        }


        var accessToken = context.Request.Cookies["accessToken"]
            ?? context.Request.Headers["Authorization"]
                .ToString().Replace("Bearer ", "");

        if (!string.IsNullOrEmpty(accessToken))
        {
            var principal = _tokenService.ValidateToken(accessToken);
            if (principal != null)
            {
                context.Request.Headers["X-User-Id"]    = principal.FindFirst("sub")?.Value;
                context.Request.Headers["X-Company-Id"] = principal.FindFirst("cid")?.Value;
            }
        }

        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        await _next(context);

        if (context.Response.StatusCode == 401 &&
            context.Request.Cookies.ContainsKey("refreshToken"))
        {
            var refreshed = await TryRefreshTokenAsync(context);

            if (refreshed)
            {
                buffer.SetLength(0);
                context.Response.Body = originalBody;
                context.Response.StatusCode = 200;

                var newAccessToken = context.Request.Cookies["accessToken"];
                if (!string.IsNullOrEmpty(newAccessToken))
                {
                    var principal = _tokenService.ValidateToken(newAccessToken);
                    if (principal != null)
                    {
                        context.Request.Headers["X-User-Id"]    = principal.FindFirst("sub")?.Value;
                        context.Request.Headers["X-Company-Id"] = principal.FindFirst("cid")?.Value;
                    }
                }

                await _next(context);
                return;
            }
        }

        buffer.Position = 0;
        await buffer.CopyToAsync(originalBody);
        context.Response.Body = originalBody;
    }

    private bool IsAuthEndpoint(PathString path) =>
        path.StartsWithSegments("/api/user/login")    ||
        path.StartsWithSegments("/api/user/register") ||
        path.StartsWithSegments("/api/user/refresh")  ||
        path.StartsWithSegments("/api/user/me");

    private async Task<bool> TryRefreshTokenAsync(HttpContext context)
    {
        var authUrl = _configuration["services:auth:http:0"];
        var client = _httpClientFactory.CreateClient();

        var request = new HttpRequestMessage(
            HttpMethod.Post, $"{authUrl}/api/user/refresh");
        request.Headers.Add("Cookie",
            context.Request.Headers["Cookie"].ToString());

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode) return false;

        foreach (var cookie in response.Headers
            .Where(h => h.Key == "Set-Cookie"))
        {
            context.Response.Headers.Append("Set-Cookie",
                cookie.Value.ToArray());
        }

        return true;
    }
}