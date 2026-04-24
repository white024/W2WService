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

    private bool IsAuthEndpoint(PathString path) =>
        path.StartsWithSegments("/api/user/login")    ||
        path.StartsWithSegments("/api/user/register") ||
        path.StartsWithSegments("/api/user/refresh")  ||
        path.StartsWithSegments("/api/user/me") ||
        path.StartsWithSegments("/api/user/logout/revoke");

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsAuthEndpoint(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Cookies.ContainsKey("refreshToken") && context.Request.Cookies.ContainsKey("accessToken"))
        {
            var rawRefreshToken = context.Request.Cookies["refreshToken"];
            await TriggerLogoutAsync(context, rawRefreshToken);
            ClearAuthCookies(context);
            context.Response.StatusCode = 401;
            return;
        }

        var accessToken = context.Request.Cookies["accessToken"]
            ?? context.Request.Headers["Authorization"]
                .ToString().Replace("Bearer ", "");

        if (!string.IsNullOrEmpty(accessToken))
            InjectUserHeaders(context, accessToken);


        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        await _next(context);

        if (context.Response.StatusCode == 401 && context.Request.Cookies.ContainsKey("refreshToken"))
        {
            var newAccessToken = await TryRefreshTokenAsync(context);

            if (newAccessToken != null)
            {
                context.Request.Headers["Authorization"] = $"Bearer {newAccessToken}";
                InjectUserHeaders(context, newAccessToken);

                var cookiesToKeep = context.Response.Headers["Set-Cookie"].ToList();

                buffer.SetLength(0);
                context.Response.StatusCode = 200;
                context.Response.Headers.Remove("Content-Length");
                context.Response.Body = buffer;

                foreach (var cookie in cookiesToKeep)
                    context.Response.Headers.Append("Set-Cookie", cookie);

                await _next(context);
            }
            else
            {
                var rawRefreshToken = context.Request.Cookies["refreshToken"];
                await TriggerLogoutAsync(context, rawRefreshToken);
                context.Response.StatusCode = 401;
            }
        }

        buffer.Position = 0;
        context.Response.Body = originalBody;
        await buffer.CopyToAsync(originalBody);
    }

    private void InjectUserHeaders(HttpContext context, string token)
    {
        var principal = _tokenService.ValidateToken(token);
        if (principal == null) return;
        context.Request.Headers["X-User-Id"]    = principal.FindFirst("sub")?.Value;
        context.Request.Headers["X-Company-Id"] = principal.FindFirst("cid")?.Value;
        context.Request.Headers["X-Customer-Id"] = principal.FindFirst("cusid")?.Value;
    }

    private async Task<string?> TryRefreshTokenAsync(HttpContext context)
    {
        var authUrl = _configuration["services:auth:http:0"];
        var client = _httpClientFactory.CreateClient();

        var request = new HttpRequestMessage(
            HttpMethod.Post, $"{authUrl}/api/user/refresh");
        request.Headers.Add("Cookie",
            context.Request.Headers["Cookie"].ToString());

        if (context.Request.Headers.TryGetValue("X-Api-Key", out var apiKey))
            request.Headers.TryAddWithoutValidation("X-Api-Key", apiKey.ToString());

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        string? newAccessToken = null;

        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            foreach (var cookie in cookies)
            {
                context.Response.Headers.Append("Set-Cookie", cookie);

                if (cookie.StartsWith("accessToken="))
                {
                    var raw = cookie.Split(';')[0].Substring("accessToken=".Length);
                    newAccessToken = Uri.UnescapeDataString(raw);
                }
            }
        }

        return newAccessToken;
    }

    private async Task TriggerLogoutAsync(HttpContext context, string? token)
    {
        try
        {
            var authUrl = _configuration["services:auth:http:0"];
            var client = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(
                HttpMethod.Post, $"{authUrl}/api/user/logout/revoke");
            if (context.Request.Headers.TryGetValue("X-Api-Key", out var apiKey))
                request.Headers.TryAddWithoutValidation("X-Api-Key", apiKey.ToString());

            request.Content = JsonContent.Create(new
            {
                refreshToken = token,
                userId = context.Request.Headers.TryGetValue("X-User-Id", out var uid)
                         ? uid.ToString()
                         : null
            });

            await client.SendAsync(request);

        }
        catch { }
        finally
        {
            ClearAuthCookies(context);
        }
    }

    private void ClearAuthCookies(HttpContext context)
    {
        var expired = new CookieOptions
        {
            Expires  = DateTimeOffset.UtcNow.AddDays(-1),
            HttpOnly = true,
            Secure   = true,
            SameSite = SameSiteMode.Strict,
            Path     = "/"
        };

        context.Response.Cookies.Append("accessToken", "", expired);
        context.Response.Cookies.Append("refreshToken", "", expired);
    }
}