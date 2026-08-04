using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Notifications.Interfaces;

/// <summary>
/// Notification-specific repository. Extends the base IRepository&lt;Notification&gt; contract
/// (AddAsync/Update/Remove/GetByIdAsync/Query) with the query shape the Notifications feed needs.
///
/// NOTE: The Assignments and Submissions bounded contexts create notification rows directly via
/// the inherited IRepository&lt;Notification&gt;.AddAsync — do not change these signatures.
/// </summary>
public interface INotificationRepository : IRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetForUserAsync(Guid userId, bool unreadOnly, CancellationToken cancellationToken = default);
}
