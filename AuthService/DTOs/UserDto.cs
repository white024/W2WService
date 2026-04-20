using AuthService.DTOs;
using Shared.Enums;
using System.Text.Json.Serialization;

public class DeviceInfoBase
{
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceId { get; set; }
}

public class UserIdentityBase : DeviceInfoBase
{
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
}

public class UserInsertDto : UserIdentityBase
{
    public string? InviteToken { get; set; }
    public UserRole? Role { get; set; } = UserRole.Customer;
    public string? Custom1 { get; set; }
    public string? Custom2 { get; set; }
    public string? Custom3 { get; set; }
    public string? Custom4 { get; set; }
}

public class UserUpdateDto
{
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Custom1 { get; set; }
    public string? Custom2 { get; set; }
    public string? Custom3 { get; set; }
    public string? Custom4 { get; set; }
}

public class LoginDto : DeviceInfoBase
{
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? CompanyId { get; set; }
}

public class UserChangePasswordDto
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}


public class LogoutDto
{
    public string? DeviceId { get; set; }
    public string? CompanyId { get; set; }
    public bool AllDevices { get; set; }
}

public class UserDto : DeviceInfoBase
{
    public string UserName { get; set; } = null!;
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? PhoneNumber { get; set; }
    public string Email { get; set; } = null!;
    public CompanyDto? ActiveCompany { get; set; }
    public UserRole? Role { get; set; } = UserRole.Customer;
    public DateTime? LastLogin { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Token { get; set; } = null!;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RefreshToken { get; set; }
}
public class UserSummaryDto
{
    public string? UserName { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? MaskedEmail { get; set; }  
    public string? MaskedPhone { get; set; }
    public CompanyDto? ActiveCompany { get; set; }
    public UserRole? Role { get; set; }
}
