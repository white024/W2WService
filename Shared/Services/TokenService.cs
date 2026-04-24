using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Shared.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateAccessToken(string userId, int? companyId, int? customerId = null)
    {
        var key = new SymmetricSecurityKey(
         System.Text.Encoding.UTF8.GetBytes(_config?["Jwt:Key"]!)
         );

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var claims = new List<Claim>
        {
            new Claim("sub", userId)
        };

        if (companyId.HasValue)
            claims.Add(new Claim("cid", companyId.Value.ToString()));

        if (customerId.HasValue)
            claims.Add(new Claim("cusid", customerId.Value.ToString()));

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

    //public async Task<(string rawToken, RefreshToken entity)> CreateRefreshToken(
    //string userId,
    //string companyId,
    //string? deviceId = null,
    //string? ipAddress = null,
    //string? userAgent = null)
    //{
    //    var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    //    var entity = new RefreshToken
    //    {
    //        UserId = userId,
    //        CompanyId = companyId,
    //        TokenHash = HashToken(rawToken),
    //        ExpiresAt = DateTime.UtcNow.AddDays(
    //            _config.GetValue<int>("Jwt:RefreshTokenDays", 30)
    //        ),
    //        CreatedAt = DateTime.UtcNow,
    //        DeviceId = deviceId,
    //        IpAddress = ipAddress,
    //        UserAgent = userAgent
    //    };

    //    return (rawToken, entity);
    //}

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        handler.InboundClaimTypeMap.Clear();

        try
        {
            return handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)),
                ValidateIssuer   = true,
                ValidIssuer      = _config["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience    = _config["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew        = TimeSpan.Zero
            }, out _);
        }
        catch
        {
            return null;
        }
    }
    public bool VerifyRefreshToken(string rawToken, string storedHash)
    {
        return HashToken(rawToken) == storedHash;
    }

    public static string HashToken(string token)
    {
        byte[] bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}