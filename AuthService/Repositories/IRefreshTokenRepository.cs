// AuthService/Repositories/IRefreshTokenRepository.cs
using AuthService.Models;

namespace AuthService.Repositories;

public interface IRefreshTokenRepository
{
    /// <summary>Yeni refresh token kaydeder.</summary>
    Task SaveAsync(RefreshToken token);

    /// <summary>TokenHash ile refresh token getirir.</summary>
    Task<RefreshToken?> GetByHashAsync(string tokenHash);

    /// <summary>Kullanıcının tüm aktif tokenlarını getirir.</summary>
    //Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);

    /// <summary>Token'ı iptal eder, isteğe bağlı yeni token ID'si ile değiştirir.</summary>
    Task RevokeAsync(string tokenId, string? replacedById = null);

    /// <summary>Güvenlik ihlalinde kullanıcının tüm tokenlarını iptal eder.</summary>
    Task RevokeAllForUserAsync(string userId);

    /// <summary>Süresi dolmuş ve iptal edilmiş tokenları temizler. (Manuel TTL)</summary>
    Task DeleteExpiredAsync();
}