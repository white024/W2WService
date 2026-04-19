using AuthService.Models;
using MongoDB.Driver;
using Shared.Indexes;

namespace AuthService.Indexes;

public class UserIndexInitializer : IIndexInitializer
{
    private readonly IMongoCollection<User> _collection;

    public UserIndexInitializer(IMongoDatabase database)
    {
        _collection = database.GetCollection<User>("User");
    }

    public async Task InitializeAsync()
    {
        await _collection.Indexes.CreateOneAsync(
            new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(x => x.UserName),
                new CreateIndexOptions { Unique = true, Name = "idx_username" }));

        await _collection.Indexes.CreateOneAsync(
            new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(x => x.Email),
                new CreateIndexOptions { Unique = true, Name = "idx_email" }));

        await _collection.Indexes.CreateOneAsync(
            new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(x => x.PhoneNumber),
                new CreateIndexOptions { Unique = true, Name = "idx_phone" }));
        await _collection.Indexes.CreateOneAsync(
            new CreateIndexModel<User>(
                Builders<User>.IndexKeys
                    .Ascending("companies.companyId")
                    .Ascending("companies.isActive"),
                new CreateIndexOptions { Name = "idx_companies" }));
    }
}