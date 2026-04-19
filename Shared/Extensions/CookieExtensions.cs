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
}
