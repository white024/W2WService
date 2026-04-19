using AuthService.Models;
using MongoDB.Driver;
using Shared.Repositories.Base;

namespace AuthService.Repositories;

public class InviteRepository : MongoRepositoryBase<Invite>, IInviteRepository
{
    protected override string GetId(Invite e) => e.Id;
    public InviteRepository(IMongoDatabase database) : base(database, "Invite")
    {
    }

}
