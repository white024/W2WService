using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Enums;

namespace AuthService.Models;


[BsonIgnoreExtraElements]

public class Company
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int? ErpRef { get; set; } = null!;
    public string? CompanyName { get; set; } = null!; 
    public string? Description { get; set; }
    public string? Details { get; set; } = null!;
    public string? Address { get; set; } = null!;
    public string? Custom1 { get; set; } = null!;
    public string? Custom2 { get; set; } = null!;
    public string? Custom3 { get; set; } = null!;
    public string? Custom4 { get; set; } = null!;
}
