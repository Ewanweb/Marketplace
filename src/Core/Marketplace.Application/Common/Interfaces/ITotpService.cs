namespace Marketplace.Application.Common.Interfaces;

public interface ITotpService
{
    string GenerateSecretKey();
    string GenerateQrCodeUri(string email, string secretKey, string issuer = "Marketplace");
    bool VerifyCode(string secretKey, string code);
    IEnumerable<string> GenerateBackupCodes(int count = 8);
}
