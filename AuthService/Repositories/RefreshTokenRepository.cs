// AuthService/Repositories/MongoRefreshTokenRepository.cs
using AuthService.Models;
using MongoDB.Driver;
using Shared.Repositories.Base;
using System.Security.Cryptography;

namespace AuthService.Repositories;

public class RefreshTokenRepository : MongoRepositoryBase<RefreshToken>, IRefreshTokenRepository
{
    protected override string GetId(RefreshToken e) => e.Id;
    public RefreshTokenRepository(IMongoDatabase database, IConfiguration config) : base(database, "RefreshToken")
    {
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var tokenHashIndex = Builders<RefreshToken>.IndexKeys
            .Ascending(x => x.TokenHash);

        var ttlIndex = Builders<RefreshToken>.IndexKeys
            .Ascending(x => x.ExpiresAt);

        var userIdIndex = Builders<RefreshToken>.IndexKeys
            .Ascending(x => x.UserId);

        Collection.Indexes.CreateMany(new[]
        {
            new CreateIndexModel<RefreshToken>(
                tokenHashIndex,
                new CreateIndexOptions { Unique = true, Name = "idx_token_hash" }),

            new CreateIndexModel<RefreshToken>(
                ttlIndex,
                new CreateIndexOptions
                {
                    ExpireAfter = TimeSpan.Zero,
                    Name = "idx_ttl_expires"
                }),

            new CreateIndexModel<RefreshToken>(
                userIdIndex,
                new CreateIndexOptions { Name = "idx_user_id" }),
        });
    }



    public async Task<RefreshToken?> GetByHashAsync(string tokenHash)
    {
        return await FindOneAsync
            (x => x.TokenHash == tokenHash);
    }
    private static string HashToken(string token)
    {
        byte[] bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
    public async Task<RefreshToken?> GetByRawTokenAsync(string rawToken)
    {
        var hash = HashToken(rawToken);
        return await GetByHashAsync(hash);
    }

    public async Task RevokeAsync(RefreshToken refreshToken, string? replacedById = null)
    {
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReplacedByTokenId = replacedById;
        refreshToken.RevokedAt = DateTime.UtcNow.AddDays(7);
        await UpdateAsync(refreshToken);
    }

    public async Task RevokeAllForUserAsync(string userId)
    {
        var update = Builders<RefreshToken>.Update
            .Set(x => x.RevokedAt, DateTime.UtcNow)
            .Set(x => x.ExpiresAt, DateTime.UtcNow.AddDays(7));

        await Collection.UpdateManyAsync(
            x => x.UserId == userId && x.RevokedAt == null,
            update);
    }

    public async Task<RefreshToken?> GetActiveTokenAsync(
    string userId,
    string companyId,
    string? deviceId = null,
    string? ipAddress = null,
    string? userAgent = null)
    {


        return (await
            FindOneAsync(x => x.UserId == userId &&
            x.CompanyId == companyId &&
            x.RevokedAt == null &&
            x.ExpiresAt > DateTime.UtcNow &&
            x.DeviceId == deviceId &&
            x.IpAddress == ipAddress &&
            x.UserAgent == userAgent, q => q.SortByDescending(x => x.CreatedAt)));

    }

}