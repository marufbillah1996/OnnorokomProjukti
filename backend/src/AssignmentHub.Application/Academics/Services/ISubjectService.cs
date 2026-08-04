using AssignmentHub.Application.Academics.Dtos;
using AssignmentHub.Application.Common.Models;

namespace AssignmentHub.Application.Academics.Services;

public interface ISubjectService
{
    Task<PaginatedList<SubjectDto>> GetAllAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<SubjectDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SubjectDto> CreateAsync(CreateSubjectRequest request, CancellationToken cancellationToken = default);
    Task<SubjectDto> UpdateAsync(Guid id, UpdateSubjectRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
