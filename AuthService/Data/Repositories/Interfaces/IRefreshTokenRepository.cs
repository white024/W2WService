// IRefreshTokenRepository.cs
using AuthService.Models.Entities;
using Shared.Repositories.Interfaces;

namespace AuthService.Data.Repositories.Interfaces;

public interface IRefreshTokenRepository : IMongoRepositoryBase<RefreshToken, string>
{
    Task<RefreshToken?> GetByRawTokenAsync(string rawToken, CancellationToken ct = default);
    Task<RefreshToken?> GetActiveTokenAsync(string userId, int? companyId, string? deviceId, CancellationToken ct = default);
    Task RevokeAsync(RefreshToken token, string? replacedById = null, string? ipAddress = null, string? deviceId = null, string? userAgent = null, string? reason = null, CancellationToken ct = default);
    Task RevokeAllForUserAsync(string userId, string? ipAddress = null, string? deviceId = null, string? userAgent = null, string? reason = null, CancellationToken ct = default);
}