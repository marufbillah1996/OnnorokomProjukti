using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Users.Interfaces;

/// <summary>
/// This exact shape (GetByEmailAsync / ExistsWithEmailAsync) is depended on by the Auth
/// bounded context — do not change these two method names/signatures.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default);
}
