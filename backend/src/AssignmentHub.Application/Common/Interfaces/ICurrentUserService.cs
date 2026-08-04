using AssignmentHub.Domain.Enums;

namespace AssignmentHub.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the authenticated caller's identity, backed by HttpContext.User in
/// Infrastructure/Identity/CurrentUserService.cs. Lets Application services depend on
/// "who is calling" without taking a dependency on ASP.NET Core's HttpContext directly.
/// </summary>
public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    string Email { get; }
    UserRole Role { get; }
}
