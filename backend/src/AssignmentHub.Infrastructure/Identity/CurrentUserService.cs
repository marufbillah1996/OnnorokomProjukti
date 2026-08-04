using System.Security.Claims;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace AssignmentHub.Infrastructure.Identity;

/// <summary>
/// Implements <see cref="ICurrentUserService"/> by reading claims off the current HttpContext.User.
/// This service can be resolved outside an HTTP request (e.g. background/seed contexts), in which
/// case every member returns its safe default instead of throwing.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid UserId
    {
        get
        {
            var value = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }

    public string Email => User?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

    public UserRole Role
    {
        get
        {
            var value = User?.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(value, true, out var role) ? role : UserRole.Student;
        }
    }
}
