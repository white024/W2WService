using Microsoft.AspNetCore.Http;

namespace Shared.Extensions;

public static class HeaderExtensions
{
    public static string? GetUserId(this HttpContext context)
        => context.Request.Headers["X-User-Id"].FirstOrDefault();

    public static string? GetCompanyId(this HttpContext context)
        => context.Request.Headers["X-Company-Id"].FirstOrDefault();

    public static string? GetUserIdFromCookie(this HttpContext context, Shared.Services.TokenService tokenService)
    {
        var token = context.Request.Cookies["accessToken"];
        if (string.IsNullOrEmpty(token)) return null;

        var principal = tokenService.ValidateToken(token);
        return principal?.FindFirst("sub")?.Value;
    }

    public static string? GetRawRefreshToken(this HttpContext context)
    {
        var rawToken = context.Request.Cookies["refreshToken"];
        return rawToken;

    }
}