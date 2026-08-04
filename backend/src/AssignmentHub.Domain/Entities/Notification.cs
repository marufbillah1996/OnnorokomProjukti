using AssignmentHub.Domain.Common;
using AssignmentHub.Domain.Enums;

namespace AssignmentHub.Domain.Entities;

/// <summary>
/// Persisted backing store for real-time (SignalR) notifications, so history survives
/// a viewer being offline or refreshing the page.
/// </summary>
public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public NotificationType Type { get; set; }

    /// <summary>JSON payload describing the event (e.g. assignment id/title, submission id/marks).</summary>
    public string Payload { get; set; } = "{}";

    public bool IsRead { get; set; }
}
