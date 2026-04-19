using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Enums;
using Shared.Models;

namespace AuthService.Models;

[BsonIgnoreExtraElements]
public class User : BaseEntityModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public int? ErpRef { get; set; } = null!;
    public string? Name { get; set; } = null!;
    public string? Surname { get; set; } = null!;
    public string? UserName { get; set; } = null!;
    public string? Password { get; set; } = null!;
    public string? Email { get; set; } = null!;
    public string? PhoneNumber { get; set; } = null!;
    public List<UserCompany>? Companies { get; set; } = new();
    public UserRole? Role { get; set; } = UserRole.Customer;
    public string? Custom1 { get; set; } = null!;
    public string? Custom2 { get; set; } = null!;
    public string? Custom3 { get; set; } = null!;
    public string? Custom4 { get; set; } = null!;
    public DateTime? LastLogin { get; set; }
}
