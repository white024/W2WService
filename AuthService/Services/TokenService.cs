// AuthService/Services/TokenService.cs
using AuthService.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AuthService.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateAccessToken(string userId, string companyId)
    {
        var key = new SymmetricSecurityKey(
         System.Text.Encoding.UTF8.GetBytes(_config?["Jwt:Key"]!)
         );

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );
        var claims = new[]
        {
            new Claim("sub", userId),
            new Claim("cid", companyId),
        };

        var token = new JwtSecurityToken(
            issuer: _config?["Jwt:Issuer"],
            audience: _config?["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                _config!.GetValue<int>("Jwt:AccessTokenMinutes", 15)
            ),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<(string rawToken, RefreshToken entity)> CreateRefreshToken(
    string userId,
    string companyId,
    string? deviceId = null,
    string? ipAddress = null,
    string? userAgent = null)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var entity = new RefreshToken
        {
            UserId = userId,
            CompanyId = companyId,
            TokenHash = HashToken(rawToken),
            ExpiresAt = DateTime.UtcNow.AddDays(
                _config.GetValue<int>("Jwt:RefreshTokenDays", 30)
            ),
            CreatedAt = DateTime.UtcNow,
            DeviceId = deviceId,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        return (rawToken, entity);
    }
    public bool VerifyRefreshToken(string rawToken, string storedHash)
    {
        return HashToken(rawToken) == storedHash;
    }

    private static string HashToken(string token)
    {
        byte[] bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}