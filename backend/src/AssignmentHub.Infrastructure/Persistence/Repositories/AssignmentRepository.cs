using AssignmentHub.Application.Assignments.Interfaces;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Infrastructure.Persistence.Repositories;

public class AssignmentRepository : Repository<Assignment>, IAssignmentRepository
{
    public AssignmentRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
