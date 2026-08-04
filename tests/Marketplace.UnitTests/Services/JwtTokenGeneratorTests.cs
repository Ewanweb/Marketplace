using System.IdentityModel.Tokens.Jwt;
using Marketplace.Domain.Entities;
using Marketplace.Identity.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Marketplace.UnitTests.Services;

public class JwtTokenGeneratorTests
{
    private readonly JwtTokenGenerator _jwtTokenGenerator;

    public JwtTokenGeneratorTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", "SuperSecretKeyForMarketplaceSecurityService2026!" },
            { "JwtSettings:Issuer", "MarketplaceAPI" },
            { "JwtSettings:Audience", "MarketplaceClients" },
            { "JwtSettings:ExpiryMinutes", "15" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _jwtTokenGenerator = new JwtTokenGenerator(configuration);
    }

    [Fact]
    public void GenerateAccessToken_ShouldProduceValidTokenWithClaims()
    {
        // Arrange
        var user = User.Create("test@example.com", "HashedPass123!");
        var roles = new[] { "Admin" };
        var permissions = new[] { "Users.Read", "Products.Create" };

        // Act
        var tokenString = _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(tokenString));

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        Assert.Equal("MarketplaceAPI", token.Issuer);
        Assert.Contains("MarketplaceClients", token.Audiences);
        Assert.Equal(user.Email, token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnRandomBase64String()
    {
        // Act
        var token1 = _jwtTokenGenerator.GenerateRefreshToken();
        var token2 = _jwtTokenGenerator.GenerateRefreshToken();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token1));
        Assert.False(string.IsNullOrWhiteSpace(token2));
        Assert.NotEqual(token1, token2);
    }
}
