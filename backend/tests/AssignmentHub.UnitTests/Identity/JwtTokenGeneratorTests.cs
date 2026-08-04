using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Domain.Entities;
using AssignmentHub.Domain.Enums;
using AssignmentHub.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AssignmentHub.UnitTests.Identity;

public class JwtTokenGeneratorTests
{
    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "unit-test-signing-secret-at-least-32-chars-long",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience",
            ["Jwt:AccessTokenMinutes"] = "15"
        }).Build();

    [Fact]
    public void GenerateAccessToken_ReturnsExpiryAndClaimsDerivedFromUser()
    {
        var fixedUtcNow = new DateTime(2026, 8, 4, 12, 0, 0, DateTimeKind.Utc);
        var mockDateTimeProvider = new Mock<IDateTimeProvider>();
        mockDateTimeProvider.Setup(p => p.UtcNow).Returns(fixedUtcNow);

        var sut = new JwtTokenGenerator(BuildConfiguration(), mockDateTimeProvider.Object);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Jane Doe",
            Email = "jane@example.com",
            Role = UserRole.Teacher
        };

        var result = sut.GenerateAccessToken(user);

        result.ExpiresAtUtc.Should().Be(fixedUtcNow.AddMinutes(15));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        jwt.Claims.Should().ContainSingle(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        jwt.Claims.Should().ContainSingle(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        jwt.Claims.Should().ContainSingle(c => c.Type == ClaimTypes.Role && c.Value == "Teacher");
    }

    [Fact]
    public void GenerateRefreshTokenValue_ReturnsNonEmptyValue_AndDiffersBetweenCalls()
    {
        var sut = new JwtTokenGenerator(BuildConfiguration(), Mock.Of<IDateTimeProvider>());

        var first = sut.GenerateRefreshTokenValue();
        var second = sut.GenerateRefreshTokenValue();

        first.Should().NotBeNullOrEmpty();
        second.Should().NotBeNullOrEmpty();
        first.Should().NotBe(second);
    }
}
