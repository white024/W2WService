using Microsoft.AspNetCore.Http;

namespace Shared.Extensions;

public static class CookieExtensions
{
    public static string? AppendRefreshToken(this HttpResponse response, string token)
    {
        response.Cookies.Append("refreshToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(30)
        });
        return null!;
    }
    public static string? AppendAccessToken(this HttpResponse response, string token)
    {
        response.Cookies.Append("accessToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure   = true,
            SameSite = SameSiteMode.Strict,
            Expires  = DateTimeOffset.UtcNow.AddMinutes(15)
        });
        return null!;
    }

    public static void ClearAuthCookies(this HttpResponse response)
    {
        response.Cookies.Delete("accessToken");
        response.Cookies.Delete("refreshToken");
    }
}
