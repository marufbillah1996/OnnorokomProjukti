using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AssignmentHub.Infrastructure.Realtime;

/// <summary>
/// SignalR hub used to push real-time notifications to authenticated users. The Api layer wires
/// up the endpoint (e.g. "/hubs/notifications") and configures JWT bearer authentication to accept
/// the access token from the query string for this hub's path, since browsers cannot set the
/// Authorization header on a WebSocket upgrade request.
///
/// This hub is push-only from the server side (via IHubContext&lt;NotificationsHub&gt; in
/// SignalRRealtimeNotifier) — clients never invoke methods on it directly. Each connection is added
/// to a group named after the connected user's id so server-side code can target a specific user.
/// </summary>
[Authorize]
public class NotificationsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        await base.OnConnectedAsync();
    }
}
