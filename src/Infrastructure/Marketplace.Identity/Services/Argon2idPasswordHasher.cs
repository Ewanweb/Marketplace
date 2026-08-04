using Isopoh.Cryptography.Argon2;
using Marketplace.Application.Common.Interfaces;

namespace Marketplace.Identity.Services;

public sealed class Argon2idPasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return Argon2.Hash(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        return Argon2.Verify(hash, password);
    }
}
