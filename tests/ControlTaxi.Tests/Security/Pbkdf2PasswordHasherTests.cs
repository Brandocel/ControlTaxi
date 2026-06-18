using ControlTaxi.Infrastructure.Security;
using FluentAssertions;
using Xunit;

namespace ControlTaxi.Tests.Security;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Hash_then_Verify_with_correct_password_returns_true()
    {
        var hash = _hasher.Hash("Secreta123");
        _hasher.Verify("Secreta123", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_with_wrong_password_returns_false()
    {
        var hash = _hasher.Hash("Secreta123");
        _hasher.Verify("incorrecta", hash).Should().BeFalse();
    }

    [Fact]
    public void Hash_produces_different_output_each_time_due_to_salt()
    {
        _hasher.Hash("misma").Should().NotBe(_hasher.Hash("misma"));
    }

    [Fact]
    public void Verify_with_malformed_hash_returns_false()
    {
        _hasher.Verify("x", "no-es-un-hash-valido").Should().BeFalse();
    }
}
