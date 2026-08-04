using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Application.Notifications.Dtos;
using AssignmentHub.Application.Notifications.Interfaces;
using AssignmentHub.Application.Notifications.Mappings;
using AssignmentHub.Domain.Exceptions;

namespace AssignmentHub.Application.Notifications.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(INotificationRepository notificationRepository, IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetForUserAsync(Guid userId, bool unreadOnly, CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetForUserAsync(userId, unreadOnly, cancellationToken);
        return notifications.Select(n => n.ToDto()).ToList();
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken)
            ?? throw new KeyNotFoundException("Notification not found.");

        if (notification.UserId != userId)
        {
            throw new ForbiddenOperationException("You can only mark your own notifications as read.");
        }

        notification.IsRead = true;

        _notificationRepository.Update(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
