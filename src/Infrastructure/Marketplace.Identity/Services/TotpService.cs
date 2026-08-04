using Marketplace.Application.Common.Interfaces;
using OtpNet;

namespace Marketplace.Identity.Services;

public sealed class TotpService : ITotpService
{
    public string GenerateSecretKey()
    {
        var secretKey = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(secretKey);
    }

    public string GenerateQrCodeUri(string email, string secretKey, string issuer = "Marketplace")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(secretKey);

        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedEmail = Uri.EscapeDataString(email);

        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secretKey}&issuer={encodedIssuer}&digits=6";
    }

    public bool VerifyCode(string secretKey, string code)
    {
        if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        try
        {
            var secretBytes = Base32Encoding.ToBytes(secretKey);
            var totp = new Totp(secretBytes);
            return totp.VerifyTotp(code.Trim(), out _, VerificationWindow.RfcSpecifiedNetworkDelay);
        }
        catch
        {
            return false;
        }
    }

    public IEnumerable<string> GenerateBackupCodes(int count = 8)
    {
        var codes = new List<string>();
        for (var i = 0; i < count; i++)
        {
            var code = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            codes.Add(code);
        }
        return codes;
    }
}
