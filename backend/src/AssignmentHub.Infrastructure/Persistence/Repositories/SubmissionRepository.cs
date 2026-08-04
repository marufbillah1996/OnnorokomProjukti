using AssignmentHub.Application.Submissions.Interfaces;
using AssignmentHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentHub.Infrastructure.Persistence.Repositories;

public class SubmissionRepository : Repository<Submission>, ISubmissionRepository
{
    public SubmissionRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    // Tracked — used to detect duplicates before insert; kept consistent with other lookup-then-mutate methods.
    public Task<Submission?> GetByAssignmentAndStudentAsync(Guid assignmentId, Guid studentId, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId, cancellationToken);
}
