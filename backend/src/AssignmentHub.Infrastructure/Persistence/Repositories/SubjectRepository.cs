using AssignmentHub.Application.Academics.Interfaces;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Infrastructure.Persistence.Repositories;

public class SubjectRepository : Repository<Subject>, ISubjectRepository
{
    public SubjectRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
