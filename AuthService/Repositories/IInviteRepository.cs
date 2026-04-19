using AuthService.Models;
using Shared.Repositories.Interfaces;

namespace AuthService.Repositories
{
    public interface IInviteRepository : IMongoRepositoryBase<Invite,string> 
    {
    }
}
