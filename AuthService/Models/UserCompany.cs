using MongoDB.Bson.Serialization.Attributes;
using Shared.Enums;

namespace AuthService.Models;

[BsonIgnoreExtraElements]

public class UserCompany
{
    public string Id { get; set; } = null!;
    public string? CompanyName { get; set; }    
    public bool IsActive { get; set; } = false;
    public CompanyRole Role { get; set; } = CompanyRole.Viewer;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}