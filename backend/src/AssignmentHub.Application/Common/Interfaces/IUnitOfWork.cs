namespace AssignmentHub.Application.Common.Interfaces;

/// <summary>
/// Thin wrapper over the EF Core DbContext's SaveChangesAsync. Repositories only track changes
/// (Add/Update/Remove); services call SaveChangesAsync once per use case, keeping transaction
/// boundaries explicit and at the Application layer, not scattered across repositories.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
