// RefreshTokenRepository.cs
using AuthService.Data.Repositories.Interfaces;
using AuthService.Models.Entities;
using MongoDB.Driver;
using Shared.Enums;
using Shared.Repositories.Base;
using Shared.Services;

namespace AuthService.Data.Repositories;

public class RefreshTokenRepository : MongoRepositoryBase<RefreshToken>, IRefreshTokenRepository
{
    protected override string GetId(RefreshToken entity) => entity.Id;

    public RefreshTokenRepository(IMongoDatabase database)
        : base(database, "RefreshTokens") { }

    public async Task<RefreshToken?> GetByRawTokenAsync(
        string rawToken, CancellationToken ct = default)
    {
        var hash = TokenService.HashToken(rawToken);
        return await FindOneAsync(x => x.TokenHash == hash, ct: ct);
    }

    public async Task<RefreshToken?> GetActiveTokenAsync(
        string userId, int? companyId, string? deviceId,
        CancellationToken ct = default)
        => await FindOneAsync(x =>
            x.UserId    == userId    &&
            x.CompanyId == companyId &&
            x.DeviceId  == deviceId  &&
            x.RevokedAt == null      &&
            x.ExpiresAt > DateTime.UtcNow,
            ct: ct);

    public async Task RevokeAsync(
        RefreshToken token,
        string? replacedById = null,
        string? ipAddress = null,
        string? deviceId = null,
        string? userAgent = null,
        string? reason = null,
        CancellationToken ct = default)
    {
        token.Revoke(replacedById, ipAddress, deviceId, userAgent, reason);
        await UpdateAsync(token, ct);
    }

    public async Task RevokeAllForUserAsync(
        string userId,
        string? ipAddress = null,
        string? deviceId = null,
        string? userAgent = null,
        string? reason = null,
        CancellationToken ct = default)
    {
        var filter = Builders<RefreshToken>.Filter.And(
            Builders<RefreshToken>.Filter.Eq(x => x.UserId, userId),
            Builders<RefreshToken>.Filter.Eq(x => x.RevokedAt, null));

        var update = Builders<RefreshToken>.Update
            .Set(x => x.RevokedAt, DateTime.UtcNow)
            .Set(x => x.IsActive, false)
            .Set(x => x.RevokedByIp, ipAddress)
            .Set(x => x.RevokedByDeviceId, deviceId)
            .Set(x => x.RevokedByUserAgent, userAgent)
            .Set(x => x.RevokedReason, reason != null ? (RevokeReason?)Enum.Parse(typeof(RevokeReason), reason) : null);

        await Collection.UpdateManyAsync(filter, update, cancellationToken: ct);
    }
}