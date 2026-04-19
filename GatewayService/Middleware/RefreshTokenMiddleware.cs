namespace GatewayService.Middleware;

public class RefreshTokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public RefreshTokenMiddleware(
        RequestDelegate next,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _next = next;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Auth endpoint'lerine dokunma
        if (context.Request.Path.StartsWithSegments("/api/user/login") ||
            context.Request.Path.StartsWithSegments("/api/user/register") ||
            context.Request.Path.StartsWithSegments("/api/user/refresh") ||
            context.Request.Path.StartsWithSegments("/api/user/me"))
        {
            await _next(context);
            return;
        }

        // Response'u buffer'la
        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        await _next(context);

        if (context.Response.StatusCode == 401 &&
            context.Request.Cookies.ContainsKey("refreshToken"))
        {
            var authUrl = _configuration["services:auth:http:0"];
            var client = _httpClientFactory.CreateClient();

            var refreshRequest = new HttpRequestMessage(
                HttpMethod.Post, $"{authUrl}/api/user/refresh");
            refreshRequest.Headers.Add("Cookie",
                context.Request.Headers["Cookie"].ToString());

           
            var refreshResponse = await client.SendAsync(refreshRequest);

            if (refreshResponse.IsSuccessStatusCode)
            {
                foreach (var cookie in refreshResponse.Headers
                    .Where(h => h.Key == "Set-Cookie"))
                {
                    context.Response.Headers.Append("Set-Cookie",
                        cookie.Value.ToArray());
                }

                buffer.SetLength(0);
                context.Response.StatusCode = 200;
            }
        }

        buffer.Position = 0;
        await buffer.CopyToAsync(originalBody);
        context.Response.Body = originalBody;
    }
}