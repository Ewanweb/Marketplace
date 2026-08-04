using Marketplace.Domain.Entities;
using Xunit;

namespace Marketplace.UnitTests.Domain;

public class UserDomainTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldReturnUserWithDefaults()
    {
        // Arrange & Act
        var user = User.Create("test@example.com", "HashedPassword123!");

        // Assert
        Assert.NotNull(user);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("HashedPassword123!", user.PasswordHash);
        Assert.False(user.IsEmailConfirmed);
        Assert.True(user.IsActive);
        Assert.False(user.IsLockedOut());
        Assert.Equal(0, user.AccessFailedCount);
    }

    [Fact]
    public void RecordFailedLoginAttempt_UnderMax_ShouldIncrementAccessFailedCountWithoutLockout()
    {
        // Arrange
        var user = User.Create("test@example.com", "HashedPassword123!");

        // Act
        user.RecordFailedLoginAttempt(5, TimeSpan.FromMinutes(15));

        // Assert
        Assert.Equal(1, user.AccessFailedCount);
        Assert.False(user.IsLockedOut());
    }

    [Fact]
    public void RecordFailedLoginAttempt_ReachingMax_ShouldLockoutUser()
    {
        // Arrange
        var user = User.Create("test@example.com", "HashedPassword123!");

        // Act
        for (var i = 0; i < 5; i++)
        {
            user.RecordFailedLoginAttempt(5, TimeSpan.FromMinutes(15));
        }

        // Assert
        Assert.Equal(5, user.AccessFailedCount);
        Assert.True(user.IsLockedOut());
    }

    [Fact]
    public void ResetFailedLoginCount_ShouldClearLockoutAndCount()
    {
        // Arrange
        var user = User.Create("test@example.com", "HashedPassword123!");
        for (var i = 0; i < 5; i++)
        {
            user.RecordFailedLoginAttempt(5, TimeSpan.FromMinutes(15));
        }

        // Act
        user.ResetFailedLoginCount();

        // Assert
        Assert.Equal(0, user.AccessFailedCount);
        Assert.False(user.IsLockedOut());
    }
}
