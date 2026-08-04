using AssignmentHub.Application.Common.Models;
using AssignmentHub.Application.Users.Dtos;

namespace AssignmentHub.Application.Users.Services;

public interface IUserService
{
    Task<PaginatedList<UserDto>> GetAllAsync(PaginationParams pagination, string? roleFilter, CancellationToken cancellationToken = default);
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
