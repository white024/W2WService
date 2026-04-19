using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthService.Models;

[BsonIgnoreExtraElements]

public class Invite
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string CompanyId { get; set; } = null!;
    public string? CompanyName { get; set; }
    public DateTime ExpireAt { get; set; }
    public bool IsUsed { get; set; }
}