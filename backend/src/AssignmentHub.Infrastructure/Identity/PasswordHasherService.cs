using AssignmentHub.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AssignmentHub.Infrastructure.Identity;

/// <summary>
/// Implements <see cref="IPasswordHasher"/> using ASP.NET Core Identity's PasswordHasher&lt;T&gt; (PBKDF2).
/// There is no real identity "TUser" model in this project, so a dummy object instance is used —
/// PasswordHasher&lt;TUser&gt; never actually inspects the user object during hashing/verification,
/// it is only a generic-type formality.
/// </summary>
public class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password)
    {
        return _hasher.HashPassword(new object(), password);
    }

    public bool Verify(string hashedPassword, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(new object(), hashedPassword, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
