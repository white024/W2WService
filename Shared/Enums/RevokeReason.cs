namespace Shared.Enums;

public enum RevokeReason
{
    Logout,          // normal çıkış
    Refresh,         // yeni token alındı, eski revoke edildi
    PasswordChange,  // şifre değişti, tüm tokenlar revoke
    AdminRevoke,     // admin tarafından
    Suspicious,      // şüpheli aktivite
    Expired          // süresi doldu
}