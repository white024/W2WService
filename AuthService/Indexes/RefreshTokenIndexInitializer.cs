using AuthService.Models;
using MongoDB.Driver;
using Shared.Indexes;

namespace AuthService.Indexes;

public class RefreshTokenIndexInitializer : IIndexInitializer
{
    private readonly IMongoCollection<RefreshToken> _collection;

    public RefreshTokenIndexInitializer(IMongoDatabase database)
    {
        _collection = database.GetCollection<RefreshToken>("RefreshToken");
    }

    public async Task InitializeAsync()
    {
        await _collection.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<RefreshToken>(
                Builders<RefreshToken>.IndexKeys.Ascending(x => x.TokenHash),
                new CreateIndexOptions { Unique = true, Name = "idx_token_hash" }),

            new CreateIndexModel<RefreshToken>(
                Builders<RefreshToken>.IndexKeys.Ascending(x => x.ExpiresAt),
                new CreateIndexOptions
                {
                    ExpireAfter = TimeSpan.Zero,
                    Name = "idx_ttl_expires"
                }),

            new CreateIndexModel<RefreshToken>(
                Builders<RefreshToken>.IndexKeys.Ascending(x => x.UserId),
                new CreateIndexOptions { Name = "idx_user_id" })
        });
    }
}