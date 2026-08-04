using AssignmentHub.Application.Academics.Interfaces;
using AssignmentHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentHub.Infrastructure.Persistence.Repositories;

/// <summary>
/// StudentClass has no BaseEntity/Id (composite key StudentId+ClassId), so unlike the other
/// repositories here this does not inherit Repository&lt;T&gt; — it talks to the DbSet directly.
/// </summary>
public class StudentClassRepository : IStudentClassRepository
{
    private readonly AppDbContext _dbContext;
    private readonly DbSet<StudentClass> _set;

    public StudentClassRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _set = dbContext.StudentClasses;
    }

    public Task<bool> ExistsAsync(Guid studentId, Guid classId, CancellationToken cancellationToken = default) =>
        _set.AnyAsync(sc => sc.StudentId == studentId && sc.ClassId == classId, cancellationToken);

    // Does not call SaveChanges — the calling service owns the unit-of-work boundary.
    public async Task AddAsync(StudentClass enrollment, CancellationToken cancellationToken = default) =>
        await _set.AddAsync(enrollment, cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetClassIdsForStudentAsync(Guid studentId, CancellationToken cancellationToken = default) =>
        await _set.AsNoTracking()
            .Where(sc => sc.StudentId == studentId)
            .Select(sc => sc.ClassId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetStudentIdsForClassAsync(Guid classId, CancellationToken cancellationToken = default) =>
        await _set.AsNoTracking()
            .Where(sc => sc.ClassId == classId)
            .Select(sc => sc.StudentId)
            .ToListAsync(cancellationToken);
}
