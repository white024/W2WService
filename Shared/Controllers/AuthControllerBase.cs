using Microsoft.AspNetCore.Mvc;
using Shared.Extensions;
using Shared.Services;

namespace Shared.Controllers;

public abstract class AuthControllerBase : ControllerBase
{
    protected string? DeviceId => Request.Headers["X-Device-Id"].ToString();
    protected string? IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();
    protected string? UserAgent => Request.Headers["User-Agent"].ToString();
    protected string? RawRefreshToken => Request.Cookies["refreshToken"];
    protected string? RawAccessToken => Request.Cookies["accessToken"];

    protected string? GetRequesterId(TokenService tokenService) =>
        HttpContext.GetUserId() ?? HttpContext.GetUserIdFromCookie(tokenService);
}