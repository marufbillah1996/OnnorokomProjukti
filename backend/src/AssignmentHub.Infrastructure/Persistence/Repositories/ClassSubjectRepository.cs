using AssignmentHub.Application.Academics.Interfaces;
using AssignmentHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentHub.Infrastructure.Persistence.Repositories;

public class ClassSubjectRepository : Repository<ClassSubject>, IClassSubjectRepository
{
    public ClassSubjectRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    // Tracked — callers reassign TeacherId on the returned entity before SaveChanges.
    public Task<ClassSubject?> GetAsync(Guid classId, Guid subjectId, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(cs => cs.ClassId == classId && cs.SubjectId == subjectId, cancellationToken);
}
