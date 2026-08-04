namespace AssignmentHub.Application.Common.Interfaces;

/// <summary>Implemented in Infrastructure using ASP.NET Core Identity's PasswordHasher&lt;T&gt; (PBKDF2). Never a custom hash.</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string hashedPassword, string providedPassword);
}
