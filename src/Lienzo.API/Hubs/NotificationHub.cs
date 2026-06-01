using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Lienzo.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (!string.IsNullOrEmpty(role))
            await Groups.AddToGroupAsync(Context.ConnectionId, role);

        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);

        await base.OnConnectedAsync();
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (!string.IsNullOrEmpty(role))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, role);

        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);

        await base.OnDisconnectedAsync(exception);
    }
}
