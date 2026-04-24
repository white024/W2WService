using Shared.Enums;
using System.Text.Json.Serialization;

namespace AuthService.Models.DTOs;

public class UserAgentDto
{
    public string? DeviceId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
public class UserDto : UserAgentDto
{
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public UserRole UserRole { get; set; } = UserRole.Customer;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public bool IsEmailVerified { get; set; } = false;
    public bool IsPhoneVerified { get; set; } = false;
    public int FailedLoginCount { get; set; } = 0;
    public string? PreferredLanguage { get; set; }
    public string? TimeZone { get; set; }
    public DateTime? LastLogin { get; set; }
    public int? CompanyId { get; set; }
    public int? CustomerId { get; set; }

    [JsonIgnore]
    public string Token { get; set; } = null!;
    [JsonIgnore]
    public string RefreshToken { get; set; } = null!;
}

public class UserCreateDto : UserDto
{
    public string Password { get; set; } = null!;
    public string? InviteCode { get; set; }
}

public class UserUpdateDto : UserDto
{
    public string? Password { get; set; }
}
public class UserLoginDto : UserAgentDto
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public int CompanyId { get; set; }

}

public class LogoutDto : UserAgentDto
{
    public int? CompanyId { get; set; }
    public bool AllDevices { get; set; }
}
public class UserChangePasswordDto
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}
