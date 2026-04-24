using AuthService.Data.Repositories.Interfaces;
using AuthService.Models.Entities;
using MongoDB.Driver;
using Shared.Repositories.Base;

namespace AuthService.Data.Repositories
{
    public class UserRepository : MongoRepositoryBase<User>, IUserRepository
    {
        protected override string GetId(User entity) => entity.Id;

        public UserRepository(IMongoDatabase database)
            : base(database, "Users") { }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
            => await FindOneAsync(x => x.Email == email, ct: ct);

        public async Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default)
            => await FindOneAsync(x => x.UserName == userName, ct: ct);

        public async Task<User?> GetByPhoneAsync(string phone, CancellationToken ct = default)
            => await FindOneAsync(x => x.PhoneNumber == phone, ct: ct);

        public async Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default)
            => await FindOneAsync(x => x.Email == email, ct: ct) != null;
    }
}
