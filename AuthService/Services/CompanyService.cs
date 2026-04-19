using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Shared.Enums;
using Shared.Models;

namespace AuthService.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    public CompanyService(ICompanyRepository companyRepository, IMapper mapper, IUserRepository userRepository)
    {
        _companyRepository = companyRepository;
        _mapper = mapper;
        _userRepository=userRepository;
    }

    public async Task<Company> CreateIfNotFoundCompanyAsync(string? companyId, string? companyName)
    {
        if (!string.IsNullOrEmpty(companyId))
        {
            var company = await _companyRepository.FindOneAsync(x => x.Id == companyId);
            if (company == null)
                throw new Exception("Company not found");
            return company;
        }

        var newCompany = new Company
        {
            CompanyName = string.IsNullOrWhiteSpace(companyName) ? "Default Company" : companyName,
        };

        var result = await _companyRepository.AddAsync(newCompany);
        if (result == null) throw new Exception("Company could not be saved");
        return result;
    }
    public async Task<ReturnObject<CompanyDto>?> AddCompany(string userId, [FromBody] CompanyDto dto)
    {
        Company company = _mapper.Map<Company>(dto);
        Company result = await _companyRepository.AddAsync(company);
        await AddCompanyToUserAsync(userId, result.Id);
        return ReturnObject<CompanyDto>.Success(dto);
    }

    public async Task<ReturnObject<List<CompanyDto>>?> GetAllCompaniesAsync()
    {
        var companies = (await _companyRepository.GetAllAsync()).ToList();
        var companyDtos = _mapper.Map<List<CompanyDto>>(companies);
        return ReturnObject<List<CompanyDto>>.Success(companyDtos);
    }

    public async Task<ReturnObject<CompanyDto>?> UpdateCompanyAsync(CompanyDto dto, string id)
    {
        var company = await _companyRepository.FindOneAsync(x => x.Id == id);
        if (company == null) throw new KeyNotFoundException("Company not found");

        _mapper.Map(dto, company);
       Company result = await _companyRepository.UpdateAsync(company);

        await _userRepository.UpdateCompanySnapshotAsync(result);

        return ReturnObject<CompanyDto>.Success(_mapper.Map<CompanyDto>(company));
    }
    public async Task<ReturnObject<bool>> DeleteCompanyAsync(string id)
    {
        var company = await _companyRepository.FindOneAsync(x => x.Id == id);
        if (company == null) throw new KeyNotFoundException("Company not found");

        await _companyRepository.DeleteAsync(id);

        // User snapshot'larından da kaldır
        await _userRepository.RemoveCompanyFromAllUsersAsync(id);

        return ReturnObject<bool>.Success(true);
    }

    public async Task<ReturnObject<UserDto>> AddCompanyToUserAsync(string userId, string companyId)
    {
        var user = await _userRepository.FindOneAsync(x => x.Id == userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        if (user.Companies != null && user.Companies.Any(x => x.Id == companyId))
            throw new InvalidOperationException("Company already assigned to user");

        var company = await _companyRepository.FindOneAsync(x => x.Id == companyId);
        if (company == null) throw new KeyNotFoundException("Company not found");

        var hasActive = user.Companies != null && user.Companies.Any(x => x.IsActive);

        user.Companies ??= new List<UserCompany>();
        user.Companies.Add(new UserCompany
        {
            Id   = company.Id,
            CompanyName = company.CompanyName,
            Role        = CompanyRole.Viewer,
            IsActive    = !hasActive,
            JoinedAt    = DateTime.UtcNow
        });

        await _userRepository.UpdateAsync(user);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.ActiveCompany = _mapper.Map<CompanyDto>(user.Companies.First(x => x.IsActive));
        return ReturnObject<UserDto>.Success(userDto);
    }

    public async Task<ReturnObject<UserDto>> RemoveCompanyFromUserAsync(string userId, string companyId)
    {
        var user = await _userRepository.FindOneAsync(x => x.Id == userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        var userCompany = user.Companies?.FirstOrDefault(x => x.Id == companyId);
        if (userCompany == null) throw new KeyNotFoundException("Company not assigned to user");

        if (user.Companies!.Count == 1)
            throw new InvalidOperationException("User must have at least one company");

        user.Companies.Remove(userCompany);

        if (userCompany.IsActive)
            user.Companies.First().IsActive = true;

        await _userRepository.UpdateAsync(user);

        var userDto = _mapper.Map<UserDto>(user);
        userDto.ActiveCompany = _mapper.Map<CompanyDto>(user.Companies.First(x => x.IsActive));
        return ReturnObject<UserDto>.Success(userDto);
    }
}