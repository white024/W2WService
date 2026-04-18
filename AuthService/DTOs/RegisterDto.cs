using AuthService.Models;
using Shared.Enums;

namespace AuthService.DTOs;

public class RegisterDto
{
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Email { get; set; } = null!;
    public UserRole? Role { get; set; } = UserRole.Customer;
    public string? Custom1 { get; set; } = null!;
    public string? Custom2 { get; set; } = null!;
    public string? Custom3 { get; set; } = null!;
    public string? Custom4 { get; set; } = null!;
}
