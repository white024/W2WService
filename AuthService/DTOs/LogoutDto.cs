namespace AuthService.DTOs;

public class LogoutDto
{
    public string? UserId { get; set; }
    public string? DeviceId { get; set; }
    public string? CompanyId { get; set; }
    public bool AllDevices { get; set; }
}