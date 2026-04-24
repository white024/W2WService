

namespace Shared.Enums;

public enum UserStatus
{
    Active,
    Suspended,   // admin tarafından askıya alındı
    Deleted,     // soft delete
    PendingVerification  // email doğrulanmadı
}