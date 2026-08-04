using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AssignmentHub.Infrastructure.Persistence.Repositories;

/// <summary>
/// Baseline CRUD implementation shared by every concrete repository. Query() is no-tracking
/// (read/list projections); GetByIdAsync is tracked (load-then-mutate-then-save flows) — see
/// the IRepository&lt;T&gt; contract in Application/Common/Interfaces for the full rationale.
/// </summary>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext DbContext;
    protected readonly DbSet<T> DbSet;

    public Repository(AppDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<T>();
    }

    public virtual Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public virtual IQueryable<T> Query() => DbSet.AsNoTracking();

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await DbSet.AddAsync(entity, cancellationToken);

    public virtual void Update(T entity) => DbSet.Update(entity);

    public virtual void Remove(T entity) => DbSet.Remove(entity);
}
