using AssignmentHub.Application.Academics.Dtos;
using AssignmentHub.Application.Common.Models;

namespace AssignmentHub.Application.Academics.Services;

public interface IClassService
{
    Task<PaginatedList<ClassDto>> GetAllAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<ClassDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ClassDto> CreateAsync(CreateClassRequest request, CancellationToken cancellationToken = default);
    Task<ClassDto> UpdateAsync(Guid id, UpdateClassRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
