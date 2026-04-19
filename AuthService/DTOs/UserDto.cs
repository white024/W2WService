using AuthService.Models;
using Shared.Enums;
using System.Text.Json.Serialization;

namespace AuthService.DTOs;

public class UserDto
{
    public string? Name { get; set; } = null!;
    public CompanyDto? ActiveCompany { get; set; } = null!;
    public UserRole? Role { get; set; } = UserRole.Customer;
    public DateTime? LastLogin { get; set; }
    public string Token { get; set; } = null!;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RefreshToken { get; set; } = null!;

}
