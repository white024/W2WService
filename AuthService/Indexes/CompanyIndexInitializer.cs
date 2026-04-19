// AuthService/Indexes/CompanyIndexInitializer.cs
using AuthService.Models;
using MongoDB.Driver;
using Shared.Indexes;

namespace AuthService.Indexes;

public class CompanyIndexInitializer : IIndexInitializer
{
    private readonly IMongoCollection<Company> _collection;

    public CompanyIndexInitializer(IMongoDatabase database)
    {
        _collection = database.GetCollection<Company>("Company");
    }

    public async Task InitializeAsync()
    {
        await _collection.Indexes.CreateOneAsync(
            new CreateIndexModel<Company>(
                Builders<Company>.IndexKeys.Ascending(x => x.CompanyName),
                new CreateIndexOptions { Name = "idx_company_name" }));
    }
}