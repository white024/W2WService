using AuthService.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Repositories.Base;

namespace AuthService.Repositories;

public class UserRepository : MongoRepositoryBase<User>, IUserRepository
{
    protected override string GetId(User e) => e.Id;
    public UserRepository(IMongoDatabase database) : base(database, "User")
    {

    }
    public async Task SetActiveCompanyAsync(string userId, string companyId)
    {
        await Collection.UpdateOneAsync(
          x => x.Id == userId,
          Builders<User>.Update.Set("Companies.$[].IsActive", false));

        var arrayFilter = new[]
        {
        new BsonDocumentArrayFilterDefinition<BsonDocument>(
            new BsonDocument("elem._id", companyId))
    };

        await Collection.UpdateOneAsync(
            x => x.Id == userId,
            Builders<User>.Update.Combine(
                Builders<User>.Update.Set("Companies.$[elem].IsActive", true),
                Builders<User>.Update.Set(x => x.LastLogin, DateTime.UtcNow)
            ),
            new UpdateOptions { ArrayFilters = arrayFilter });
    }

    public async Task RemoveCompanyFromAllUsersAsync(string companyId)
    {
        await Collection.UpdateManyAsync(
            x => x.Companies != null && x.Companies.Any(c => c.Id == companyId),
            Builders<User>.Update.PullFilter(
                x => x.Companies,
                c => c.Id == companyId));
    }
    public async Task UpdateCompanySnapshotAsync(Company company)
    {
        var arrayFilter = new[]
        {
        new BsonDocumentArrayFilterDefinition<BsonDocument>(
            new BsonDocument("elem._id", company.Id))
    };

        await Collection.UpdateManyAsync(
            x => x.Companies != null && x.Companies.Any(c => c.Id == company.Id),
            Builders<User>.Update
                .Set("Companies.$[elem].CompanyName", company.CompanyName),
            new UpdateOptions { ArrayFilters = arrayFilter });
    }

}
