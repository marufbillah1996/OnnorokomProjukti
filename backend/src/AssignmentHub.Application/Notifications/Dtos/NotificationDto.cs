namespace AssignmentHub.Application.Notifications.Dtos;

/// <summary>
/// Read model for a persisted notification row, exposed to a user's own notification feed.
/// </summary>
public record NotificationDto(Guid Id, string Type, string Payload, bool IsRead, DateTime CreatedAt);
