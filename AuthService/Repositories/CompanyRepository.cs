using AuthService.Models;
using MongoDB.Driver;
using Shared.Repositories.Base;

namespace AuthService.Repositories;

public class CompanyRepository : MongoRepositoryBase<Company>, ICompanyRepository
{
    protected override string GetId(Company e) => e.Id;
    public CompanyRepository(IMongoDatabase database) : base(database, "Company")
    {
    }
}
