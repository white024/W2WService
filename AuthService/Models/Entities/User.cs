using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Enums;
using Shared.Models;

namespace AuthService.Models.Entities;

[BsonIgnoreExtraElements]
public class User : BaseEntityModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string? UserName { get; set; }
    public string? InviteCode { get; set; }
    public string Password { get; set; } = null!;
    public string? PhoneNumber { get; set; }   
    public string? AvatarUrl { get; set; }    
    public UserRole UserRole { get; set; } = UserRole.Customer;
    public UserStatus Status { get; set; } = UserStatus.Active;  
    public bool IsEmailVerified { get; set; } = false;
    public bool IsPhoneVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpiry { get; set; }
    public int FailedLoginCount { get; set; } = 0; 
    public DateTime? LockoutEnd { get; set; }     
    public string? PreferredLanguage { get; set; }  
    public string? TimeZone { get; set; }
    public string? LastLoginIp { get; set; }
    public string? DeviceId { get; set; }
    public string? UserAgent { get; set; }
    public string? Custom1 { get; set; }
    public string? Custom2 { get; set; }
    public string? Custom3 { get; set; }

    public void SetLastLogin(string? ipAddress)
    {
        LastLogin   = DateTime.UtcNow;
        LastLoginIp = ipAddress;
    }

    public void IncrementFailedLogin()
    {
        FailedLoginCount++;
        if (FailedLoginCount >= 5)
            LockoutEnd = DateTime.UtcNow.AddMinutes(15);
    }

    public void ResetFailedLogin()
    {
        FailedLoginCount = 0;
        LockoutEnd       = null;
    }

    public bool IsLockedOut =>
        LockoutEnd.HasValue && LockoutEnd > DateTime.UtcNow;

    public DateTime? LastLogin { get; private set; }
}