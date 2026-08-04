using AssignmentHub.Application.Submissions.Interfaces;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Infrastructure.Persistence.Repositories;

public class GradeAuditLogRepository : Repository<GradeAuditLog>, IGradeAuditLogRepository
{
    public GradeAuditLogRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
