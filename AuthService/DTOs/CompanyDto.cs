using Shared.Enums;

namespace AuthService.DTOs;

public class CompanyDto
{
    public string? CompanyName { get; set; } = null!;
    public string? Description { get; set; }
    public CompanyRole? CRole { get; set; } = CompanyRole.Viewer;


}
