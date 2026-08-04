using AssignmentHub.Application.Common.Models;
using AssignmentHub.Application.Submissions.Dtos;

namespace AssignmentHub.Application.Submissions.Services;

public interface ISubmissionService
{
    Task<SubmissionDto> SubmitAsync(Guid assignmentId, Guid studentId, SubmitAnswerRequest request, CancellationToken cancellationToken = default);

    Task<SubmissionDto> UpdateAsync(Guid submissionId, Guid studentId, SubmitAnswerRequest request, CancellationToken cancellationToken = default);

    Task<PaginatedList<SubmissionDto>> GetForAssignmentAsync(Guid assignmentId, PaginationParams pagination, CancellationToken cancellationToken = default);

    Task<PaginatedList<SubmissionDto>> GetForStudentAsync(Guid studentId, PaginationParams pagination, CancellationToken cancellationToken = default);

    Task<PaginatedList<SubmissionDto>> GetAllForAdminAsync(PaginationParams pagination, CancellationToken cancellationToken = default);

    Task<SubmissionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SubmissionDto> GradeAsync(Guid submissionId, Guid teacherId, GradeSubmissionRequest request, CancellationToken cancellationToken = default);

    Task<SubmissionDto> ChangeStatusAsync(Guid submissionId, Guid teacherId, ChangeSubmissionStatusRequest request, CancellationToken cancellationToken = default);

    Task<bool> IsGradableByTeacherAsync(Guid submissionId, Guid teacherId, CancellationToken cancellationToken = default);
}
