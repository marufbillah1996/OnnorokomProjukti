using AssignmentHub.Domain.Common;

namespace AssignmentHub.Application.Common.Interfaces;

/// <summary>
/// Baseline CRUD contract shared by every bounded-context repository. Query() returns an
/// IQueryable so callers can compose filtering/paging (see PaginatedList.CreateAsync) without
/// the repository needing a bespoke method per query shape.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    IQueryable<T> Query();
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
}
