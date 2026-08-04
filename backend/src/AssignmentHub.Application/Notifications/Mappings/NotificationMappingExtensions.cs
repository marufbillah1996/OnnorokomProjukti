using AssignmentHub.Application.Notifications.Dtos;
using AssignmentHub.Domain.Entities;

namespace AssignmentHub.Application.Notifications.Mappings;

public static class NotificationMappingExtensions
{
    public static NotificationDto ToDto(this Notification n) =>
        new(n.Id, n.Type.ToString(), n.Payload, n.IsRead, n.CreatedAt);
}
