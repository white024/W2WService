// AuthService/Repositories/MongoRefreshTokenRepository.cs
using AuthService.Models;
using MongoDB.Driver;

namespace AuthService.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IMongoCollection<RefreshToken> _collection;

    public RefreshTokenRepository(IConfiguration config)
    {
        string connectionString = config["MongoDB:ConnectionString"]!;
        string databaseName = config["MongoDB:Database"]!;
        string collectionName = config.GetValue<string>("MongoDB:RefreshTokenCollection", "refresh_tokens")!;

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _collection  = database.GetCollection<RefreshToken>(collectionName);

        CreateIndexes();
    }

    private void CreateIndexes()
    {
        // ✅ TokenHash ile hızlı arama
        var tokenHashIndex = Builders<RefreshToken>.IndexKeys
            .Ascending(x => x.TokenHash);

        // ✅ Süresi dolmuş tokenları otomatik sil (TTL Index)
        var ttlIndex = Builders<RefreshToken>.IndexKeys
            .Ascending(x => x.ExpiresAt);

        // ✅ UserId ile tüm tokenları bulma
        var userIdIndex = Builders<RefreshToken>.IndexKeys
            .Ascending(x => x.UserId);

        _collection.Indexes.CreateMany(new[]
        {
            new CreateIndexModel<RefreshToken>(
                tokenHashIndex,
                new CreateIndexOptions { Unique = true, Name = "idx_token_hash" }),

            new CreateIndexModel<RefreshToken>(
                ttlIndex,
                new CreateIndexOptions
                {
                    ExpireAfter = TimeSpan.Zero,  // ExpiresAt gelince otomatik sil
                    Name = "idx_ttl_expires"
                }),

            new CreateIndexModel<RefreshToken>(
                userIdIndex,
                new CreateIndexOptions { Name = "idx_user_id" }),
        });
    }

    public async Task SaveAsync(RefreshToken token)
    {
        await _collection.InsertOneAsync(token);
    }

    public async Task<RefreshToken?> GetByHashAsync(string tokenHash)
    {
        return await _collection
            .Find(x => x.TokenHash == tokenHash)
            .FirstOrDefaultAsync();
    }

    public async Task RevokeAsync(string tokenId, string? replacedById = null)
    {
        var update = Builders<RefreshToken>.Update
            .Set(x => x.RevokedAt, DateTime.UtcNow)
            .Set(x => x.ReplacedByTokenId, replacedById);

        await _collection.UpdateOneAsync(
            x => x.Id == tokenId,
            update);
    }

    public async Task RevokeAllForUserAsync(string userId)
    {
        // ✅ Güvenlik ihlalinde tüm tokenları iptal et
        var update = Builders<RefreshToken>.Update
            .Set(x => x.RevokedAt, DateTime.UtcNow);

        await _collection.UpdateManyAsync(
            x => x.UserId == userId && x.RevokedAt == null,
            update);
    }
    public async Task DeleteExpiredAsync()
    {
       
    }
}