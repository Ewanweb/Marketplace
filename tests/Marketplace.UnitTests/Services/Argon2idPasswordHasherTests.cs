using Marketplace.Identity.Services;
using Xunit;

namespace Marketplace.UnitTests.Services;

public class Argon2idPasswordHasherTests
{
    private readonly Argon2idPasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnValidArgon2Hash()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash = _hasher.HashPassword(password);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.NotEqual(password, hash);
        Assert.StartsWith("$argon2id$", hash);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hash = _hasher.HashPassword(password);

        // Act
        var isValid = _hasher.VerifyPassword(password, hash);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_WithWrongPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "SecurePassword123!";
        var wrongPassword = "WrongPassword123!";
        var hash = _hasher.HashPassword(password);

        // Act
        var isValid = _hasher.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifyPassword_WithSeededAdminPassword_ShouldReturnTrue()
    {
        var password = "Admin@123456";
        var hash = "$argon2id$v=19$m=65536,t=3,p=1$t3spA9wh4NUB1wk5kT9ejw$WIU+dzsDyvQ2XZcKoeWI3KMXvsMTCfQtZ1DrlWd8P4w";
        var isValid = _hasher.VerifyPassword(password, hash);
        Assert.True(isValid);
    }
}
