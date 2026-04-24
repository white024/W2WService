using AuthService.Models.Entities;
using Shared.Repositories.Interfaces;

namespace AuthService.Data.Repositories.Interfaces
{
    public interface IUserRepository : IMongoRepositoryBase<User, string>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default);
        Task<User?> GetByPhoneAsync(string phone, CancellationToken ct = default);
        Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default);
    }
}
