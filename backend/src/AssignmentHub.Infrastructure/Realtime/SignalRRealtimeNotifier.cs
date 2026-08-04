using AssignmentHub.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AssignmentHub.Infrastructure.Realtime;

/// <summary>
/// SignalR-backed implementation of <see cref="IRealtimeNotifier"/>. Pushes events to the groups
/// that <see cref="NotificationsHub"/> adds connections to (one group per user id string).
/// </summary>
public class SignalRRealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<NotificationsHub> _hubContext;

    public SignalRRealtimeNotifier(IHubContext<NotificationsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyAssignmentPublishedAsync(IReadOnlyCollection<Guid> studentUserIds, Guid assignmentId, string assignmentTitle, CancellationToken cancellationToken = default)
    {
        if (studentUserIds.Count == 0)
        {
            return;
        }

        var groupNames = studentUserIds.Select(id => id.ToString());

        await _hubContext.Clients.Groups(groupNames).SendAsync(
            "AssignmentPublished",
            new { assignmentId, assignmentTitle },
            cancellationToken: cancellationToken);
    }

    public async Task NotifySubmissionGradedAsync(Guid studentUserId, Guid submissionId, int? marksAwarded, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(studentUserId.ToString()).SendAsync(
            "SubmissionGraded",
            new { submissionId, marksAwarded },
            cancellationToken: cancellationToken);
    }
}
