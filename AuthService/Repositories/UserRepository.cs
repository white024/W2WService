using AuthService.Models;
using MongoDB.Driver;
using Shared.Repositories.Base;

namespace AuthService.Repositories;

public class UserRepository : MongoRepositoryBase<User>, IUserRepository
{
    protected override string GetId(User e) => e.Id;
    public UserRepository(IMongoDatabase database) : base(database, "User")
    {

    }

}
