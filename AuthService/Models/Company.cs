using Shared.Enums;

namespace AuthService.Models;

public class Company
{
    public string Id { get; set; } = null!;
    public int? ErpRef { get; set; } = null!;
    public string? Name { get; set; } = null!; 
    public string? Description { get; set; }
    public string? Details { get; set; } = null!;
    public string? Adress { get; set; } = null!;
    public CompanyRole? CRole { get; set; } = CompanyRole.Viewer;
    public List<Company>? SubCompanies { get; set; } = new List<Company>();
    public string? Custom1 { get; set; } = null!;
    public string? Custom2 { get; set; } = null!;
    public string? Custom3 { get; set; } = null!;
    public string? Custom4 { get; set; } = null!;
}
