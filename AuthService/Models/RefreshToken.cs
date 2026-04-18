// AuthService/Models/RefreshToken.cs
using Shared.Models;

namespace AuthService.Models;

public class RefreshToken : BaseEntityModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = null!;
    public string CompanyId { get; set; } = null!;
    public string TokenHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenId { get; set; }
    public string? DeviceId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public bool TokenIsActive
        => RevokedAt == null && DateTime.UtcNow < ExpiresAt;

}