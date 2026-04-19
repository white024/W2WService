using AuthService.Models;
using Shared.Repositories.Interfaces;

namespace AuthService.Repositories;

public interface IUserRepository : IMongoRepositoryBase<User, string>
{
    Task SetActiveCompanyAsync(string userId, string companyId);
    Task UpdateCompanySnapshotAsync(Company company);
    Task RemoveCompanyFromAllUsersAsync(string companyId);
}
