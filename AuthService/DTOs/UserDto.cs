using AuthService.Models;
using Shared.Enums;

namespace AuthService.DTOs;

public class UserDto
{
    public string? Name { get; set; } = null!;
    public CompanyDto? ActiveCompany { get; set; } = null!;
    public UserRole? Role { get; set; } = UserRole.Customer;
    public DateTime? LastLogin { get; set; }
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;

}
