namespace Shared.Extensions;

public static class MaskingExtensions
{
    public static string? MaskEmail(this string? email)
    {
        if (string.IsNullOrEmpty(email)) return null;
        var parts = email.Split('@');
        if (parts.Length != 2) return "***";

        var local = parts[0];
        var domain = parts[1];

        var masked = local.Length <= 2
            ? "***"
            : local[..2] + new string('*', local.Length - 2);

        return $"{masked}@{domain}";
        // test@mail.com → te***@mail.com
    }

    public static string? MaskPhone(this string? phone)
    {
        if (string.IsNullOrEmpty(phone)) return null;
        if (phone.Length < 7) return "***";

        return phone[..3] + new string('*', phone.Length - 6) + phone[^3..];
        // 05301234567 → 053*****567
    }

    public static string? MaskName(this string? name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (name.Length <= 1) return "*";

        return name[0] + new string('*', name.Length - 1);
        // Ahmet → A****
    }
}