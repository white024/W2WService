using AuthService.DTOs;
using AuthService.Models;
using Shared.Models;

namespace AuthService.Services;

public interface ICompanyService
{
    Task<Company> CreateIfNotFoundCompanyAsync(string? companyId, string? companyName);
    Task<ReturnObject<List<CompanyDto>>?> GetAllCompaniesAsync();
    Task<ReturnObject<CompanyDto>?> AddCompany(string userId, CompanyDto dto);
    Task<ReturnObject<CompanyDto>?> UpdateCompanyAsync(CompanyDto dto, string id);
    Task<ReturnObject<UserDto>> AddCompanyToUserAsync(string userId, string companyId);
    Task<ReturnObject<UserDto>> RemoveCompanyFromUserAsync(string userId, string companyId);
    Task<ReturnObject<bool>> DeleteCompanyAsync(string id);
}
