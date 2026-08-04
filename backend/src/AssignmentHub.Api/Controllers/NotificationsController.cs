using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Application.Notifications.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentHub.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public NotificationsController(INotificationService notificationService, ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine([FromQuery] bool unreadOnly, CancellationToken cancellationToken)
    {
        return Ok(await _notificationService.GetForUserAsync(_currentUserService.UserId, unreadOnly, cancellationToken));
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        await _notificationService.MarkAsReadAsync(id, _currentUserService.UserId, cancellationToken);
        return NoContent();
    }
}
