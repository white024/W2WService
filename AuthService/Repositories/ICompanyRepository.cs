using AuthService.Models;
using Shared.Repositories.Interfaces;

namespace AuthService.Repositories;

public interface ICompanyRepository : IMongoRepositoryBase<Company,string>
{
}
