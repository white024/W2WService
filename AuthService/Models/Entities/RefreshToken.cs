using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Enums;
using Shared.Models;

namespace AuthService.Models.Entities;

public class RefreshToken : BaseEntityModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string UserId { get; set; } = null!;
    public int? CompanyId { get; set; }
    public int? CustomerId { get; set; }
    public string TokenHash { get; set; } = null!;

    public string? DeviceId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenId { get; set; }

    public string? RevokedByIp { get; set; }
    public string? RevokedByDeviceId { get; set; }
    public string? RevokedByUserAgent { get; set; }
    public RevokeReason? RevokedReason { get; set; }  // Logout, Refresh, AdminRevoke, Suspicious

    public bool IsExpired => ExpiresAt < DateTime.UtcNow;
    public bool IsRevoked => RevokedAt.HasValue;

    public void Revoke(
        string? replacedById = null,
        string? ipAddress = null,
        string? deviceId = null,
        string? userAgent = null,
        string? reason = null)
    {
        RevokedAt            = DateTime.UtcNow;
        ReplacedByTokenId    = replacedById;
        RevokedByIp          = ipAddress;
        RevokedByDeviceId    = deviceId;
        RevokedByUserAgent   = userAgent;
        RevokedReason        = (RevokeReason?)Enum.Parse(typeof(RevokeReason), reason ?? string.Empty);
        IsActive             = false;
    }
}