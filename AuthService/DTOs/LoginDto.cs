using AuthService.Models;

namespace AuthService.DTOs;

public class LoginDto
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? CompanyId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceId { get; set; }
}
