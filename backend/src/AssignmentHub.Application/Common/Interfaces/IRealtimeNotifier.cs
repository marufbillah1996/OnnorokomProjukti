namespace AssignmentHub.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the live push channel (implemented via SignalR in
/// Infrastructure/Realtime/SignalRRealtimeNotifier.cs). Application services call this after
/// persisting a Notification row so the Domain/Application layers never reference SignalR types.
/// </summary>
public interface IRealtimeNotifier
{
    Task NotifyAssignmentPublishedAsync(IReadOnlyCollection<Guid> studentUserIds, Guid assignmentId, string assignmentTitle, CancellationToken cancellationToken = default);

    Task NotifySubmissionGradedAsync(Guid studentUserId, Guid submissionId, int? marksAwarded, CancellationToken cancellationToken = default);
}
