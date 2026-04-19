// AuthService/Repositories/IRefreshTokenRepository.cs
using AuthService.Models;
using Shared.Repositories.Interfaces;

namespace AuthService.Repositories;

public interface IRefreshTokenRepository : IMongoRepositoryBase<RefreshToken, string>
{
    /// <summary>TokenHash ile refresh token getirir.</summary>
    Task<RefreshToken?> GetByHashAsync(string tokenHash);

    /// <summary>Kullanıcının tüm aktif tokenlarını getirir.</summary>
    //Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);

    /// <summary>Token'ı iptal eder, isteğe bağlı yeni token ID'si ile değiştirir.</summary>
    Task RevokeAsync(RefreshToken refreshToken, string? replacedById = null);

    /// <summary>Güvenlik ihlalinde kullanıcının tüm tokenlarını iptal eder.</summary>
    Task RevokeAllForUserAsync(string userId);

    /// <summary>Süresi dolmuş ve iptal edilmiş tokenları temizler. (Manuel TTL)</summary>

    Task<RefreshToken?> GetByRawTokenAsync(string refreshToken);

    Task<RefreshToken?> GetActiveTokenAsync(
    string userId,
    string companyId,
    string? deviceId = null,
    string? ipAddress = null,
    string? userAgent = null);
}