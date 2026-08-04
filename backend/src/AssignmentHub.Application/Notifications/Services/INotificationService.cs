using AssignmentHub.Application.Notifications.Dtos;

namespace AssignmentHub.Application.Notifications.Services;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetForUserAsync(Guid userId, bool unreadOnly, CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
}
