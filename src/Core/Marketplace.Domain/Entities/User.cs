namespace Marketplace.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsEmailConfirmed { get; private set; }
    public string? EmailVerificationToken { get; private set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; private set; }

    public string ReferralCode { get; private set; } = string.Empty;

    public bool IsTwoFactorEnabled { get; private set; }
    public string? TwoFactorSecret { get; private set; }
    public string? BackupCodesJson { get; private set; }

    public bool IsLockoutEnabled { get; private set; } = true;
    public DateTimeOffset? LockoutEnd { get; private set; }
    public int AccessFailedCount { get; private set; }

    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public ICollection<AffiliateReferral> AffiliateReferrals { get; private set; } = new List<AffiliateReferral>();

    private User() { }

    public static User Create(string email, string passwordHash, string fullName = "", string? phoneNumber = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User
        {
            Id = Guid.NewGuid(),
            FullName = (fullName ?? string.Empty).Trim(),
            Email = email.ToLowerInvariant().Trim(),
            PhoneNumber = phoneNumber?.Trim(),
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            IsEmailConfirmed = false,
            ReferralCode = GenerateReferralCode()
        };
    }

    private static string GenerateReferralCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEmailVerificationToken(string token, TimeSpan lifetime)
    {
        EmailVerificationToken = token;
        EmailVerificationTokenExpiresAt = DateTime.UtcNow.Add(lifetime);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordFailedLoginAttempt(int maxFailedAttempts, TimeSpan lockoutDuration)
    {
        AccessFailedCount++;
        if (IsLockoutEnabled && AccessFailedCount >= maxFailedAttempts)
        {
            LockoutEnd = DateTimeOffset.UtcNow.Add(lockoutDuration);
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetFailedLoginCount()
    {
        AccessFailedCount = 0;
        LockoutEnd = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsLockedOut()
    {
        return IsLockoutEnabled && LockoutEnd.HasValue && LockoutEnd.Value > DateTimeOffset.UtcNow;
    }

    public void SetPasswordResetToken(string token, TimeSpan lifetime)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpiresAt = DateTime.UtcNow.Add(lifetime);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newPasswordHash);
        PasswordHash = newPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnableTwoFactor(string secret, string backupCodesJson)
    {
        TwoFactorSecret = secret;
        BackupCodesJson = backupCodesJson;
        IsTwoFactorEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DisableTwoFactor()
    {
        TwoFactorSecret = null;
        BackupCodesJson = null;
        IsTwoFactorEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
