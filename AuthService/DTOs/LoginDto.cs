using AuthService.Models;

namespace AuthService.DTOs;

public class LoginDto
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public Company? company { get; set; } = null!;
}
