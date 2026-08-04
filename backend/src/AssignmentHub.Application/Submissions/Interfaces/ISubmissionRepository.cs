using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Submissions.Interfaces;

public interface ISubmissionRepository : IRepository<Submission>
{
    Task<Submission?> GetByAssignmentAndStudentAsync(Guid assignmentId, Guid studentId, CancellationToken cancellationToken = default);
}
