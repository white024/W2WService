using AuthService.DTOs;
//using AuthService.Filters;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.Attributes;
using Shared.Models;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiKeyAuth]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _companyService.GetAllCompaniesAsync();
        return Ok(result);
    }
    [HttpPost("{userId}")]
    public async Task<IActionResult> Add(string userId, [FromBody] CompanyDto dto)
    {
        ReturnObject<CompanyDto>? result = await _companyService.AddCompany(userId, dto);
        return Ok(result);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] CompanyDto dto)
    {
        var result = await _companyService.UpdateCompanyAsync(dto, id);
        return Ok(result);
    }
    [HttpPost("{userId}/company/{companyId}")]
    public async Task<IActionResult> AddCompany(string userId, string companyId)
    {
        var result = await _companyService.AddCompanyToUserAsync(userId, companyId);
        return Ok(result);
    }

    [HttpDelete("{userId}/company/{companyId}")]
    public async Task<IActionResult> RemoveCompany(string userId, string companyId)
    {
        var result = await _companyService.RemoveCompanyFromUserAsync(userId, companyId);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _companyService.DeleteCompanyAsync(id);
        return Ok(result);
    }
}