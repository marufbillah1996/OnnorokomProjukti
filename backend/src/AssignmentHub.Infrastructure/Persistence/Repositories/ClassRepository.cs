using AssignmentHub.Application.Academics.Interfaces;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Infrastructure.Persistence.Repositories;

public class ClassRepository : Repository<Class>, IClassRepository
{
    public ClassRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
