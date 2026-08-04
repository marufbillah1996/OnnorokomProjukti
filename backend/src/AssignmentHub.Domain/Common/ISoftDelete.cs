namespace AssignmentHub.Domain.Common;

/// <summary>
/// Marks an entity as eligible for soft delete. Only entities implementing this
/// are subject to the EF Core global query filter (see Infrastructure/Persistence/AppDbContext).
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}
