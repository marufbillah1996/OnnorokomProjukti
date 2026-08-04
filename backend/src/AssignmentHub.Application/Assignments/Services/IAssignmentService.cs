using AssignmentHub.Application.Assignments.Dtos;
using AssignmentHub.Application.Common.Models;

namespace AssignmentHub.Application.Assignments.Services;

public interface IAssignmentService
{
    Task<PaginatedList<AssignmentDto>> GetAllForAdminAsync(PaginationParams pagination, CancellationToken cancellationToken = default);

    Task<PaginatedList<AssignmentDto>> GetAllForTeacherAsync(Guid teacherId, PaginationParams pagination, CancellationToken cancellationToken = default);

    Task<PaginatedList<AssignmentDto>> GetPublishedForStudentAsync(Guid studentId, PaginationParams pagination, CancellationToken cancellationToken = default);

    Task<AssignmentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AssignmentDto> CreateAsync(Guid teacherId, CreateAssignmentRequest request, CancellationToken cancellationToken = default);

    Task<AssignmentDto> UpdateAsync(Guid id, UpdateAssignmentRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AssignmentDto> PublishAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> IsOwnedByTeacherAsync(Guid assignmentId, Guid teacherId, CancellationToken cancellationToken = default);
}
