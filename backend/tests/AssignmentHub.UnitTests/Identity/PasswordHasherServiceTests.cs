using AssignmentHub.Infrastructure.Identity;
using FluentAssertions;

namespace AssignmentHub.UnitTests.Identity;

public class PasswordHasherServiceTests
{
    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var sut = new PasswordHasherService();

        var hash = sut.Hash("MyP@ssw0rd");

        sut.Verify(hash, "MyP@ssw0rd").Should().BeTrue();
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ReturnsFalse()
    {
        var sut = new PasswordHasherService();
        var hash = sut.Hash("MyP@ssw0rd");

        sut.Verify(hash, "wrong").Should().BeFalse();
    }

    [Fact]
    public void Hash_CalledTwiceWithSameInput_ProducesDifferentSaltedHashes()
    {
        var sut = new PasswordHasherService();

        var first = sut.Hash("MyP@ssw0rd");
        var second = sut.Hash("MyP@ssw0rd");

        first.Should().NotBe(second);
    }
}
